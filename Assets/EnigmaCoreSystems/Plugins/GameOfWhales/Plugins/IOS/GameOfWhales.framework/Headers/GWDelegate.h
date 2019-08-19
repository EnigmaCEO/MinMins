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

#ifndef GWDelegate_h
#define GWDelegate_h

#import <Foundation/Foundation.h>
#import "GWSpecialOffer.h"

@class GWSpecialOffer;

/*!
 @brief GW event handling protocol
 */
@protocol GWDelegate <NSObject>

/*!
 @brief When new special offer appeared
 @param specialOffer Appeared special offer
 */
- (void)specialOfferAppeared:(nonnull GWSpecialOffer *)specialOffer;

/*!
 @brief When some special offer disappeared
 @param specialOffer Disappeared special offer
 */
- (void)specialOfferDisappeared:(nonnull GWSpecialOffer *)specialOffer;

- (void)futureSpecialOfferAppeared:(nonnull GWSpecialOffer *)specialOffer;

/*!
 @brief When user get push notification
 @param specialOffer If push campaing has special offer
 @param camp Campaign identifier
 @param title Push notification title
 @param message Push notification message
 */
- (void)onPushDelivered:(nullable GWSpecialOffer *)specialOffer
                   camp:(nonnull NSString *)camp
                  title:(nonnull NSString *)title
                message:(nonnull NSString *)message;

/*!
 @brief When user purchase verified on server
 @param transactionID Payment transaction identifier
 @param state Verification state
 */
- (void)onPurchaseVerified:(nonnull NSString *)transactionID
                     state:(nonnull NSString *)state;

- (void)onAdLoaded;
- (void)onAdLoadFailed;
- (void)onAdClosed;

- (void)onInitialized;
@end

#endif
