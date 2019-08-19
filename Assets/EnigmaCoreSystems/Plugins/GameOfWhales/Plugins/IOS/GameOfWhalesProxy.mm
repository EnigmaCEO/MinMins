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
static const char* gw_empty_string = "";

@implementation GameOfWhalesProxy
+ (void) load
{
    try
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
    catch (...) {
        NSLog(@"GameOfWhalesProxy:load");
    }
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
    try
    {
        NSLog(@"GameOfWhalesProxy didReceiveRemoteNotification %@", notification);
        
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
    catch (...) {
        NSLog(@"GameOfWhalesProxy:didReceiveRemoteNotification");
    }
}

-(void)unitySendMethod:(NSString*)method
{
    try
    {
        UnitySendMessage((const char*)[gw_listenerName UTF8String], (const char*)[method UTF8String], gw_empty_string);
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:unitySendMethod");
    }
}

- (void)unitySendMethod:(NSString*)method param:(NSDictionary<NSString*, id>*)param{
    try
    {
        NSString* data = nil;
        if (param){
            NSError *error = nil;
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:param
                                                               options:(NSJSONWritingOptions) NSJSONWritingPrettyPrinted                                                         error:&error];
            if (error) {
                NSLog(@"GameOfWhalesProxy JSON error: %@", error.localizedDescription);
                return;
            }
            data = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        }
        UnitySendMessage((const char*)[gw_listenerName UTF8String],
                         (const char*)[method UTF8String],
                         (const char*)[data UTF8String]);
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:unitySendMethod");
    }
}

- (void) didRegisterForRemoteNotificationsWithDeviceToken:(NSNotification *)notification {
    try
    {
        NSLog(@"GameOfWhalesProxy::didRegisterForRemoteNotificationsWithDeviceToken");
        
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
    catch (...) {
        NSLog(@"GameOfWhalesProxy:didRegisterForRemoteNotificationsWithDeviceToken");
    }
}


- (void) ApplicationDidFinishLaunchingNotification:(NSNotification *)notification
{
    try
    {
        NSDictionary * launchOptions = [notification userInfo];
        gw_launchOptions = [[NSDictionary alloc] initWithDictionary:launchOptions];
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:ApplicationDidFinishLaunchingNotification");
    }
}

-(NSDictionary *) offerToDict:(GWSpecialOffer*)specialOffer
{
    try
    {
        NSDictionary* dict = @{
                               @"camp":specialOffer.campaign,
                               @"product":specialOffer.product,
                               @"countFactor": @(specialOffer.countFactor),
                               @"priceFactor": @(specialOffer.priceFactor),
                               @"finishedAt": @([specialOffer.finishedAt timeIntervalSince1970] * 1000),
                               @"activatedAt": @([specialOffer.activatedAt timeIntervalSince1970] * 1000),
                               @"payload":specialOffer.payload,
                               @"custom":specialOffer.customValues,
                               @"redeemable":@(specialOffer.redeemable)
                               };
        
        return dict;
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:offerToDict");
    }
    return NULL;
}

- (void)specialOfferAppeared:(GWSpecialOffer *)specialOffer
{
    try
    {
        NSDictionary * dict = [self offerToDict: specialOffer];
        [self unitySendMethod:@"Internal_OnSpecialOfferAppeared" param:dict];
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:specialOfferAppeared");
    }
}

- (void)specialOfferDisappeared:(GWSpecialOffer *)specialOffer
{
    try
    {
        UnitySendMessage([gw_listenerName UTF8String], "Internal_OnSpecialOfferDisappeared", [specialOffer.product UTF8String]);
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:specialOfferDisappeared");
    }
}

- (void)futureSpecialOfferAppeared:(GWSpecialOffer *)specialOffer
{
    try
    {
        NSDictionary * dict = [self offerToDict: specialOffer];
        NSLog(@"GameOfWhalesProxy futureSpecialOfferAppeared %@", dict);
        [self unitySendMethod:@"Internal_OnFutureSpecialOfferAppeared" param:dict];
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:futureSpecialOfferAppeared");
    }
}

- (void)onPushDelivered:(nullable GWSpecialOffer*) offer camp:(nonnull NSString *)camp title:(nonnull NSString*)title message:(nonnull NSString*)message
{
    try
    {
        NSLog(@"GameOfWhalesProxy: Internal_OnPushDelivered:  %@ %@ %@", camp, title, message);
        
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
    catch (...) {
        NSLog(@"GameOfWhalesProxy:onPushDelivered");
    }
}

- (void)onPurchaseVerified:(NSString *)transactionID state:(NSString *)state
{
    try
    {
        NSMutableDictionary * dict = [[NSMutableDictionary alloc] initWithCapacity:5];
        [dict setValue:transactionID forKey:@"transactionID" ];
        [dict setValue:state forKey:@"verifyState" ];
        [self unitySendMethod:@"Internal_OnPurchaseVerified" param:dict];
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:onPurchaseVerified");
    }
}

- (void)onAdClosed {
    UnityPause(NO);
    [self unitySendMethod:@"Internal_OnAdClosed"];
}


- (void)onAdLoadFailed {
    try
    {
        [self unitySendMethod:@"Internal_OnAdLoadFailed"];
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:onAdLoadFailed");
    }
}


- (void)onAdLoaded {
    try
    {
        [self unitySendMethod:@"Internal_OnAdLoaded"];
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:onAdLoaded");
    }
}

- (void)onInitialized {
 
    try
    {
        [self unitySendMethod:@"Internal_OnInitialized"];
    }
    catch (...) {
        NSLog(@"GameOfWhalesProxy:onInitialized");
    }
}




- (void)specialOffersUpdated
{
    // [self unitySendMethod:@"OnSpecialOfferUpdate" param:nil];
}
@end

extern "C" {
    void gw_initialize(const char* gameKey, const char* listenerName, const char* version, bool debug, bool nonPersonal)
    {
        try
        {
            NSString *nsGameKey = [[NSString alloc] initWithUTF8String:gameKey];
            NSString *nsVersion = [[NSString alloc] initWithUTF8String:version];
            gw_listenerName = [[NSString alloc] initWithUTF8String:listenerName];
            GW_PLATFORM = @"unity";
            GW_VERSION = nsVersion;
            [GW InitializeWithGameKey:nsGameKey :gw_launchOptions :debug :nonPersonal];
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
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_initialize");
        }
    }
    
    void gw_acquire(const char* currency, const char* amount, const char* source, const char* number, const char* place)
    {
        try
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
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_acquire");
        }
    }
    
    void gw_consume(const char* currency, const char* number, const char* sink, const char* amount, const char* place)
    {
        try
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
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_converting");
        }
    }
    
    void gw_converting(const char* resources, const char *place)
    {
        try
        {
            NSString *nsResources = [[NSString alloc] initWithUTF8String:resources];
            NSString *nsPlace = [[NSString alloc] initWithUTF8String:place];
            [GW ConvertingWithString:nsResources place:nsPlace];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_converting");
        }
    }
    
    void gw_profile(const char* params)
    {
        try
        {
            NSString *nsParams = [[NSString alloc] initWithUTF8String:params];
            [GW ProfileWithString:nsParams];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_profile");
        }
    }
    
    void gw_updateToken(const char* token, const char* provider)
    {
        try
        {
            NSString *nsToken = [[NSString alloc] initWithUTF8String:token];
            NSString *nsProvider = [[NSString alloc] initWithUTF8String:provider];
            
            [GW RegisterDeviceTokenWithString:nsToken provider:nsProvider];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_updateToken");
        }
    }

    void gw_purchase(const char *product, const char *currency, double price)
    {
        try
        {
            NSString * nsProduct = [[NSString alloc] initWithUTF8String:product];
            NSString * nsCurrency = [[NSString alloc] initWithUTF8String:currency];
            
            [GW Purchase:nsProduct :nsCurrency :price];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_purchase");
        }
    }
    
    void gw_inAppPurchased(const char* sku, double price, const char* currency, const char* transactionID, const char* receipt)
    {
        try
        {
            NSString * nsSku = [[NSString alloc] initWithUTF8String:sku];
            NSString * nsCurrency = [[NSString alloc] initWithUTF8String:currency];
            NSString * nsTransactionID = [[NSString alloc] initWithUTF8String:transactionID];
            NSString * nsReceipt = [[NSString alloc] initWithUTF8String:receipt];
            
            [GW InAppPurchased:nsSku :price :nsCurrency :nsTransactionID :nsReceipt];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_inAppPurchased");
        }
    }
    
    void gw_reportError(const char* message, const char* stacktrace)
    {
        try
        {
            NSString * nsMessage = [[NSString alloc] initWithUTF8String:message];
            NSString * nsStacktrace = [[NSString alloc] initWithUTF8String:stacktrace];
            [GW ReportError:nsMessage :nsStacktrace];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_reportError");
        }
    }
    
    void gw_showAd()
    {
        if ([GW IsAdLoaded])
        {
            UnityPause(YES);
            [GW ShowAd];
        }
    }
    
    void gw_loadAd()
    {
        try
        {
            [GW LoadAd];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_loadAd");
        }
    }
    
    bool gw_isAdLoaded()
    {
        try
        {
            return [GW IsAdLoaded];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_isAdLoaded");
        }
        return false;
    }
    
    void gw_pushReacted(const char* camp)
    {
        try
        {
            NSString *nsCampID = [[NSString alloc] initWithUTF8String:camp];
            [GW ReactedRemoteNotificationWithCampaign:nsCampID];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_pushReacted");
        }
    }
    
    char* gw_cStringCopy(const char* string)
    {
        try
        {
            if (string == NULL)
                return NULL;
            
            char* res = (char*)malloc(strlen(string) + 1);
            strcpy(res, string);
            
            return res;
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_cStringCopy");
        }
        return NULL;
    }
    
    char * gw_getProperties()
    {
        try
         {
         NSMutableDictionary * dict = [GW GetProperties];
         
         NSString* data = nil;
         if (dict){
         NSError *error = nil;
         NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict
         options:(NSJSONWritingOptions) NSJSONWritingPrettyPrinted                                                         error:&error];
         if (error) {
         NSLog(@"gGameOfWhalesProxy::w_getProperties JSON error: %@", error.localizedDescription);
         }
         data = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
         return gw_cStringCopy([data UTF8String]);
         }
         }
         catch (...) {
         NSLog(@"GameOfWhalesProxy:gw_getProperties");
         }
        return NULL;
    }
    
    char * gw_getServerTime()
    {
        try
        {
            NSDate * date = [GW GetServerTime];
            NSNumber * time = [NSNumber numberWithDouble:date.timeIntervalSince1970 * 1000.0];
            return gw_cStringCopy([[time stringValue] UTF8String]);
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_getServerTime");
        }
        
        return NULL;
    }
    
    
    void gw_setPushNotificationsEnable(bool value)
    {
        try
        {
            [GW SetPushNotificationsEnable:value];
        }
        catch (...) {
            NSLog(@"GameOfWhalesProxy:gw_setPushNotificationsEnable");
        }
    }
}

