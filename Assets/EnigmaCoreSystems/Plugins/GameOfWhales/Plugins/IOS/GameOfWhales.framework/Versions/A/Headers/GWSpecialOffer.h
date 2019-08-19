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

#ifndef GWSpecialOffer_h
#define GWSpecialOffer_h


#import <Foundation/Foundation.h>

/*!
 @brief Special offer description
 */
@interface GWSpecialOffer : NSObject

/*!
 @brief Campaign identifier, using for feedback
 */
@property(nonatomic, nonnull) NSString *campaign;

/*!
 @brief What product identifier have special offer
 */
@property(nonatomic, nonnull) NSString *product;

/*!
 @brief Price discount option.
 */
@property(nonatomic) float priceFactor;

/*!
 @brief Bonus option.
 */
@property(nonatomic) float countFactor;

/*!
 @brief Is special offer redeemable.
 */
@property(nonatomic) bool redeemable;

/*!
 @brief When special offer will finish.
 */
@property(nonatomic, nonnull) NSDate *finishedAt;

@property(nonatomic, nonnull) NSDate *activatedAt;

/*!
 @brief Special data for developers server. Contains encrypted special offer data.
 */
@property(nonatomic, nonnull) NSString *payload;

/*!
 @brief Is special offer expired
 */
- (BOOL)isExpired;

- (BOOL)isActivated;
/*!
 @brief Does special offer have discount option
 */
- (bool)hasPriceFactor;

/*!
 @brief Does special offer have bonus option
 */
- (bool)hasCountFactor;

/*!
 @brief How much time special offer will active
 */
- (long)getLeftTime;

- (long)getLeftTimeTillActivated;

@property(nonatomic, nonnull) NSDictionary * customValues;
@end



#endif

