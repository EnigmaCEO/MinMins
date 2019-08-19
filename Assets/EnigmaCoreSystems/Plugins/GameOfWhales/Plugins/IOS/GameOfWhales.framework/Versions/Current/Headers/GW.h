/*
 * Game Of Whales SDK
 *
 * https://www.gameofwhales.com/
 *
 * Copyright © 2018 GameOfWhales. All rights reserved.
 *
 * Licence: https://github.com/Game-of-whales/GOW-SDK-IOS/blob/master/LICENSE
 *
 */

#ifndef GW_h
#define GW_h

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import "GWDelegate.h"
#import "GWSpecialOffer.h"

FOUNDATION_EXPORT NSString *_Nonnull const GW_PROVIDER_APN;
FOUNDATION_EXPORT NSString *_Nonnull const GW_PROVIDER_FCM;
FOUNDATION_EXPORT NSString *_Nonnull const GW_PROVIDER_GCM;

FOUNDATION_EXPORT NSString *_Nonnull const GW_VERIFY_STATE_LEGAL;
FOUNDATION_EXPORT NSString *_Nonnull const GW_VERIFY_STATE_ILLEGAL;
FOUNDATION_EXPORT NSString *_Nonnull const GW_VERIFY_STATE_UNDEFINED;

FOUNDATION_EXPORT NSString *_Nonnull GW_PLATFORM;
FOUNDATION_EXPORT NSString *_Nonnull GW_VERSION;

FOUNDATION_EXPORT NSString *_Nonnull const GW_AD_SHOWED;
FOUNDATION_EXPORT NSString *_Nonnull const GW_AD_LOADED;
FOUNDATION_EXPORT NSString *_Nonnull const GW_AD_CLICKED;

@interface GW : NSObject
/*!
 @brief Is Game of whales SDK initialized
 @return YES, if SDK initialized
 */
+ (BOOL)IsInitialized;

/*!
 @brief Initialize GameOfWhales SDK method with specified game key.
 @param gameKey NSString with game key, getted from https://www.gameofwhales.com
 @param launchOptions NSDictionary with launch options
 @param debug BOOL YES - if debug mode
 */
+ (void)InitializeWithGameKey:(nonnull NSString *)gameKey
                             :(nullable NSDictionary *)launchOptions
                             :(BOOL)debug
                             :(BOOL)nonPersonal;

+ (void)InitializeWithGameKey:(nonnull NSString *)gameKey
                             :(nullable NSDictionary *)launchOptions
                             :(BOOL)debug;

/*!
 @brief Initialize GameOfWhales SDK method with default game key.
 @param launchOptions NSDictionary with launch options
 @param debug BOOL YES - if debug mode
 */
+ (void)Initialize:(nullable NSDictionary *)launchOptions
                  :(BOOL)debug
                  :(BOOL)nonPersonal;

+ (void)Initialize:(nullable NSDictionary *)launchOptions
                  :(BOOL)debug;

/*!
     Load promo adversting
 */
+ (void)LoadAd;

+ (bool)IsAdLoaded;
/*!
     Show promo adversting
 */
+ (void)ShowAd;

/*!
 @brief Enable/disable debug mode
 @param debug BOOL YES/NO
 */
+ (void)SetDebug:(BOOL)debug;

/*!
 @brief Add GWDelegate
 @param delegate GWDelegate
 */
+ (void)AddDelegate:(nonnull id <GWDelegate>)delegate;

/*!
 @brief Remove GWDelegate
 @param delegate GWDelegate to remove

 */
+ (void)RemoveDelegate:(nonnull id <GWDelegate>)delegate;

/*!
 @brief Get special offer for specified product
 @param productID NSString with product identifier
 @return GWSpecialOffer
 */
+ (nullable GWSpecialOffer *)GetSpecialOffer:(nonnull NSString *)productID;

+ (nullable GWSpecialOffer *)GetFutureSpecialOffer:(nonnull NSString *)productID;

+ (nullable NSMutableDictionary *)GetProperties;
+ (nullable NSString *)GetUserGroup;

/*!
 @brief Register device token with specified push messages provider
 @param deviceToken NSData
 @param provider NSString can be GW_PROVIDER_FCM or GW_PROVIDER_APN
 */
+ (void)RegisterDeviceTokenWithData:(nonnull NSData *)deviceToken
                           provider:(nonnull NSString *)provider;

/*!
 @brief Register device token with specified push messages provider
 @param deviceToken NSString
 @param provider NSString can be GW_PROVIDER_FCM or GW_PROVIDER_APN
 */
+ (void)RegisterDeviceTokenWithString:(nonnull NSString *)deviceToken provider:(nonnull NSString *)provider;

/*!
 @brief Register purchase with transaction and product
 @param transaction SKPaymentTransaction
 @param product SKProduct
 */
+ (void)PurchaseTransaction:(nonnull SKPaymentTransaction *)transaction product:(nonnull SKProduct *)product;

/*!
 @brief Register resource converting
 @param resources NSDictionary<NSString *, NSNumber*> dictionary with converting values
 @param place NSString where converting happens
 */
+ (void)Converting:(nullable NSDictionary<NSString *, NSNumber *> *)resources
             place:(nonnull NSString *)place;

/*!
 @brief Register resource converting
 @param resources NSString with converting values
 @param place NSString where converting happens
 */
+ (void)ConvertingWithString:(nonnull NSString *)resources
                       place:(nonnull NSString *)place;

/*!
 @brief Enable/disable push notifications
 @param value BOOL
 */
+ (void)SetPushNotificationsEnable:(bool)value;

/*!
 @brief Is push notifications enabled
 @return YES, if push notifications enabled
 */
+ (bool)IsPushNotificationsEnabled;

+ (NSDate*)GetServerTime;

/*!
 @brief Register currency consume
 @param currency NSString Internal game currency to consume
 @param number NSNumber How much currency comsumed
 @param sink NSString What item we get
 @param amount NSNumber How much items we get
 @param place NSString Place, where consumation happens
 */
+ (void)ConsumeCurrency:(nonnull NSString *)currency
                 number:(nonnull NSNumber *)number
                   sink:(nonnull NSString *)sink
                 amount:(nonnull NSNumber *)amount
                  place:(nonnull NSString *)place;

/*!
 @brief Register currency acquire
 @param currency NSString Internal game currency to acquire
 @param number NSNumber How much currency acquired
 @param source NSString What source of currency
 @param amount NSNumber How much source we have
 @param place NSString Place, where consumation happens
 */
+ (void)AcquireCurrency:(nonnull NSString *)currency
                 amount:(nonnull NSNumber *)amount
                 source:(nonnull NSString *)source
                 number:(nonnull NSNumber *)number
                  place:(nonnull NSString *)place;

/*!
 @brief Register profile information
 @param params NSDictionary with profile data
 */
+ (void)Profile:(nullable NSDictionary<NSString *, id> *)params;

/*!
 @brief Register profile information
 @param params NSString with profile data
 */
+ (void)ProfileWithString:(nonnull NSString *)params;

/*!
 @brief Register error
 @param message NSString Error message
 @param stacktrace NSString Error stack trace
 */
+ (void)ReportError:(nonnull NSString *)message
        :(nonnull NSString *)stacktrace;

/*!
 @param sku NSString product identifier
 @param currency NSString transaction currency
 @param price double product price
 */
+(void)Purchase:(nonnull NSString*)product: (nonnull NSString*)currency: (double)price;


/*!
 @brief Register in app purchase
 @param sku NSString product identifier
 @param price double product price
 @param currency NSString transaction currency
 @param transactionID NSString transaction identifier
 @param receipt NSString purchases receipt
 */
+ (void)InAppPurchased:(nonnull NSString *)sku
        :(double)price
        :(nonnull NSString *)currency
        :(nonnull NSString *)transactionID
        :(nonnull NSString *)receipt;

/*!
 @brief Method must be called from ApplicationDelegate::application:didReceiveRemoteNotification:fetchCompletionHandler: This method are processing remote notification from GameOfWhales when application is in background mode
 @param application UIApplication
 @param notification NSDictionary userData
 @param handler fetchCompletionHandler
 */
+ (void)ReceivedRemoteNotification:(nonnull NSDictionary *)notification
                   withApplication:(UIApplication *_Nonnull)application
            fetchCompletionHandler:(void (^ _Nullable)(UIBackgroundFetchResult result))handler;

/*!
 @brief send information that user reacted to push
 @param notification NSDictionary received notification
 */
+ (void)ReactedRemoteNotification:(nonnull NSDictionary *)notification;

/*!
 @brief Register user reaction for push notification campaign
 @param camp NSString Current campaign
 */
+ (void)ReactedRemoteNotificationWithCampaign:(nonnull NSString *)camp;

+(void)Internal_onAdClosed;
+(void)AdEvent:(NSString *)camp action:(NSString*)action;
+(void)ExtAdEvent:(NSString *)platform revenue:(NSNumber *)revenue type:(NSString*)type unitType:(NSString*)unitType;
@end



#endif
