#include "AppDelegateListener.h"
#import "UnityAppController.h"
#import <GameOfWhales/GameOfWhales.h>

//void UnitySendMessage( const char * className, const char * methodName, const char * param );

@interface GameOfWhalesProxy : NSObject<GWDelegate>
@end


static NSDictionary * gw_launchOptions = nil;
static NSDictionary * gw_push_userInfo = nil;
static GameOfWhalesProxy * gw_proxyInstance = nil;
static NSString * gw_listenerName = nil;

@implementation GameOfWhalesProxy
+ (void) load
{
    
    gw_proxyInstance = [[GameOfWhalesProxy alloc] init];
    
    [[NSNotificationCenter defaultCenter] addObserver:gw_proxyInstance selector:@selector(ApplicationDidFinishLaunchingNotification:)
                                                 name:@"UIApplicationDidFinishLaunchingNotification" object:nil];
    
    
    
    [[NSNotificationCenter defaultCenter] addObserver:gw_proxyInstance selector:
     @selector(didRegisterForRemoteNotificationsWithDeviceToken:)
                                                 name:kUnityDidRegisterForRemoteNotificationsWithDeviceToken object: nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:gw_proxyInstance selector:
     @selector(didReceiveRemoteNotification:)
                                                 name:kUnityDidReceiveRemoteNotification object: nil];
}

-(id)init
{
    if (self = [super init])
    {
        
    }
    return self;
}


- (void)didReceiveRemoteNotification:(NSNotification*)notification;
{
    NSLog(@"PROXY didReceiveRemoteNotification %@", notification);
    
    if (notification)
    {
        UIApplication * application = [UIApplication sharedApplication];
        if (application)
        {
            if ([GW IsInitialized])
            {
                [GW ReceivedRemoteNotification:[notification userInfo] withApplication:application fetchCompletionHandler:nil];
            }
            else
            {
                gw_push_userInfo = [[NSDictionary alloc] initWithDictionary:[notification userInfo]];
            }
            
        }
        
    }
}

- (void)unitySendMethod:(NSString*)method param:(NSDictionary<NSString*, id>*)param{
    NSString* data = nil;
    if (param){
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:param
                                                           options:(NSJSONWritingOptions) NSJSONWritingPrettyPrinted                                                         error:&error];
        if (error) {
            NSLog(@"JSON error: %@", error.localizedDescription);
            return;
        }
        data = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    UnitySendMessage((const char*)[gw_listenerName UTF8String],
                     (const char*)[method UTF8String],
                     (const char*)[data UTF8String]);
}

- (void) didRegisterForRemoteNotificationsWithDeviceToken:(NSNotification *)notification {
    NSLog(@"didRegisterForRemoteNotificationsWithDeviceToken");
    
    NSData * deviceToken = (NSData *)[notification userInfo];
    const char *data = (const char *) [deviceToken bytes];
    NSMutableString *token = [NSMutableString string];
    
    for (NSUInteger i = 0; i < [deviceToken length]; i++) {
        [token appendFormat:@"%02.2hhX", data[i]];
    }
    
    NSString * tokenResult = [token copy];
    NSLog(@"GameOfWhalesProxy IOS %@", tokenResult);
    [GW RegisterDeviceTokenWithString:tokenResult provider:GW_PROVIDER_APN];
}


- (void) ApplicationDidFinishLaunchingNotification:(NSNotification *)notification
{
    NSDictionary * launchOptions = [notification userInfo];
    gw_launchOptions = [[NSDictionary alloc] initWithDictionary:launchOptions];
}

- (void)specialOfferAppeared:(GWSpecialOffer *)specialOffer
{
    NSDictionary* dict = @{
                           @"camp":specialOffer.campaign,
                           @"product":specialOffer.product,
                           @"countFactor": @(specialOffer.countFactor),
                           @"priceFactor": @(specialOffer.priceFactor),
                           @"finishedAt": @([specialOffer.finishedAt timeIntervalSince1970] * 1000),
                           @"payload":specialOffer.payload,
                           @"custom":specialOffer.customValues,
                           @"redeemable":@(specialOffer.redeemable)
                           };
    
    [self unitySendMethod:@"Internal_OnSpecialOfferAppeared" param:dict];
}

- (void)specialOfferDisappeared:(GWSpecialOffer *)specialOffer
{
    UnitySendMessage([gw_listenerName UTF8String], "Internal_OnSpecialOfferDisappeared", [specialOffer.product UTF8String]);
}

- (void)onPushDelivered:(nullable GWSpecialOffer*) offer camp:(nonnull NSString *)camp title:(nonnull NSString*)title message:(nonnull NSString*)message
{
    NSMutableDictionary * dict = [[NSMutableDictionary alloc] initWithCapacity:5];
    [dict setValue:camp forKey:@"camp"];
    [dict setValue:title forKey:@"title"];
    [dict setValue:message forKey:@"message"];
    if (offer)
    {
        [dict setValue:offer.product forKey:@"offerProduct"];
    }
    
    
    [self unitySendMethod:@"Internal_OnPushDelivered" param:dict];
}

- (void)onPurchaseVerified:(NSString *)transactionID state:(NSString *)state
{
    NSMutableDictionary * dict = [[NSMutableDictionary alloc] initWithCapacity:5];
    [dict setValue:transactionID forKey:@"transactionID" ];
    [dict setValue:state forKey:@"verifyState" ];
    [self unitySendMethod:@"Internal_OnPurchaseVerified" param:dict];
}


- (void)specialOffersUpdated
{
    // [self unitySendMethod:@"OnSpecialOfferUpdate" param:nil];
}
@end

extern "C" {
    void gw_initialize(const char* gameKey, const char* listenerName, const char* version, bool debug)
    {
        NSString *nsGameKey = [[NSString alloc] initWithUTF8String:gameKey];
        NSString *nsVersion = [[NSString alloc] initWithUTF8String:version];
        gw_listenerName = [[NSString alloc] initWithUTF8String:listenerName];
        GW_PLATFORM = @"unity";
        GW_VERSION = nsVersion;
        [GW InitializeWithGameKey:nsGameKey :gw_launchOptions :debug];
        [GW AddDelegate:gw_proxyInstance];
        
        if (gw_push_userInfo)
        {
            UIApplication * application = [UIApplication sharedApplication];
            if (application)
            {
                [GW ReceivedRemoteNotification:gw_push_userInfo withApplication:application fetchCompletionHandler:nil];
                gw_push_userInfo = nil;
            }
        }
    }
    
    void gw_acquire(const char* currency, const char* amount, const char* source, const char* number, const char* place)
    {
        NSString *nsCurrency = [[NSString alloc] initWithUTF8String:currency];
        NSString *nsSource = [[NSString alloc] initWithUTF8String:source];
        NSString *nsPlace = [[NSString alloc] initWithUTF8String:place];
        NSString *nsAmountStr = [[NSString alloc] initWithUTF8String:amount];
        NSString *nsNumberStr = [[NSString alloc] initWithUTF8String:number];
        
        NSNumberFormatter *formatter = [[NSNumberFormatter alloc]init];
        NSNumber *nsAmount = [formatter numberFromString:nsAmountStr];
        NSNumber *nsNumber = [formatter numberFromString:nsNumberStr];
        
        [GW AcquireCurrency:nsCurrency amount:nsAmount source:nsSource number:nsNumber place:nsPlace];
    }
    
    void gw_consume(const char* currency, const char* number, const char* sink, const char* amount, const char* place)
    {
        NSString *nsCurrency = [[NSString alloc] initWithUTF8String:currency];
        NSString *nsSink = [[NSString alloc] initWithUTF8String:sink];
        NSString *nsPlace = [[NSString alloc] initWithUTF8String:place];
        NSString *nsAmountStr = [[NSString alloc] initWithUTF8String:amount];
        NSString *nsNumberStr = [[NSString alloc] initWithUTF8String:number];
        
        NSNumberFormatter *formatter = [[NSNumberFormatter alloc]init];
        NSNumber *nsAmount = [formatter numberFromString:nsAmountStr];
        NSNumber *nsNumber = [formatter numberFromString:nsNumberStr];
        
        [GW ConsumeCurrency:nsCurrency number:nsNumber sink:nsSink amount:nsAmount place:nsPlace];
    }
    
    void gw_converting(const char* resources, const char *place)
    {
        NSString *nsResources = [[NSString alloc] initWithUTF8String:resources];
        NSString *nsPlace = [[NSString alloc] initWithUTF8String:place];
        [GW ConvertingWithString:nsResources place:nsPlace];
    }
    
    void gw_profile(const char* params)
    {
        NSString *nsParams = [[NSString alloc] initWithUTF8String:params];
        [GW ProfileWithString:nsParams];
    }
    
    void gw_updateToken(const char* token, const char* provider)
    {
        NSString *nsToken = [[NSString alloc] initWithUTF8String:token];
        NSString *nsProvider = [[NSString alloc] initWithUTF8String:provider];
        
        [GW RegisterDeviceTokenWithString:nsToken provider:nsProvider];
    }
    
    void gw_inAppPurchased(const char* sku, double price, const char* currency, const char* transactionID, const char* receipt)
    {
        NSString * nsSku = [[NSString alloc] initWithUTF8String:sku];
        NSString * nsCurrency = [[NSString alloc] initWithUTF8String:currency];
        NSString * nsTransactionID = [[NSString alloc] initWithUTF8String:transactionID];
        NSString * nsReceipt = [[NSString alloc] initWithUTF8String:receipt];
        
        [GW InAppPurchased:nsSku :price :nsCurrency :nsTransactionID :nsReceipt];
    }
    
    void gw_reportError(const char* message, const char* stacktrace)
    {
        NSString * nsMessage = [[NSString alloc] initWithUTF8String:message];
        NSString * nsStacktrace = [[NSString alloc] initWithUTF8String:stacktrace];
        [GW ReportError:nsMessage :nsStacktrace];
    }
    
    void gw_pushReacted(const char* camp)
    {
        NSString *nsCampID = [[NSString alloc] initWithUTF8String:camp];
        [GW ReactedRemoteNotificationWithCampaign:nsCampID];
    }
    
    char* cStringCopy(const char* string)
    {
        if (string == NULL)
            return NULL;
        
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        
        return res;
    }
    
    char * gw_getServerTime()
    {
        NSDate * date = [GW GetServerTime];
        long time = date.timeIntervalSince1970 * 1000;
        NSString * nstime = [[NSNumber numberWithLong:time] stringValue];
        return cStringCopy([nstime UTF8String]);
    }
    
    
    void gw_setPushNotificationsEnable(bool value)
    {
        [GW SetPushNotificationsEnable:value];
    }
}

