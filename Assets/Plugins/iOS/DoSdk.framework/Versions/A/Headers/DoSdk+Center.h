//
//  DoSDK+Center.h
//  DoSDK
//
//  Created by finn on 15/6/17.
//  Copyright (c) 2015年 finn. All rights reserved.
//

#import "DoSdkBase.h"

@interface DoSdk (Center)

/**
 @brief	是否有用户中心
 */
- (BOOL)isExistPlatform;

/**
 @brief	进入用户中心
 @param nFlag 预留，默认为0
 */
- (void)enterPlatform;

/**
 @brief  显示log界面
 */
- (void)showSDKInfo;
@end
