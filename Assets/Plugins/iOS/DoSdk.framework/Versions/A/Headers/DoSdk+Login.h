//
//  DoSDK+Login.h
//  DoSDK
//
//  Created by finn on 15/6/17.
//  Copyright (c) 2015年 finn. All rights reserved.
//

#import "DoSdkBase.h"

@interface DoSdk (Login)

/**
 * 判断是否已经登录平台
 */
- (BOOL)isLogined;

/**
 * 登录平台,进入登录或者注册界面入口
 * @param nFlag 标识(按位标识)预留,默认为0
 * @result 错误码
 */
- (void)login:(int)nFlag;

/**
 * 注销
 * @param nFlag 标识(按位标识) 0:标识注销但保存本地信息; 1:表示注销,并清除自动登录
 * @result 错误码
 */
- (void)logout;

/**
 * 切换帐号(logout+login),会注销当前登录的帐号
 */
- (void)switchAccount;

/**
 * 获取本次登录的token,需要登陆后才能获取
 */
- (NSString *)token;

@end
