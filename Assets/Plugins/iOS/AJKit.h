//
//  AJKit.h
//  AJKit
//
//  Created by Nadav Hertzshtark on 10/14/14.
//  Copyright (c) 2014 AppJolt. All rights reserved.
//

#import <UIKit/UIKit.h>

/**
 *  The kAPJUserClickedNotification is posted whenever user clicked on a notification. If notification has a user code, it will be added to the userInfo dictionary using the kAPJClickedNotificationUserCode key
 */
FOUNDATION_EXPORT NSString *const kAPJUserClickedNotification;
FOUNDATION_EXPORT NSString *const kAPJClickedNotificationUserCode;


/**
 *  AppJolt helps you to better engage your users
 */
@interface AJKit : NSObject

/**
 *  Launch AJKit
 *
 *  @param launchOptions App Launch Options
 */
+ (void) init:(NSString *)appKey;



/**
 * Call this method if you have user email address
 */
+(void) setEmailAddress: (NSString *) email;

+(void) optOut;

@end
