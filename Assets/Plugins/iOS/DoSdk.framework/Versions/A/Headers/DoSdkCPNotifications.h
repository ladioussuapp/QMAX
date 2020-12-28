//
//  DoSDKCPNotifications.h
//  DoSDK
//
//  Created by finn on 15/6/17.
//  Copyright (c) 2015年 finn. All rights reserved.
//

#ifdef __OBJC__
#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>
#endif

/** 初始化完成的通知 */
#define kDoSdkCPInitDidFinishNotification @"k_DoSdk_cp_init_did_finish_notification"

/** 登陆完成的通知 */
#define kDoSdkCPLoginNotification @"k_DoSdk_cp_login_notification"

/** 注销结果的通知 */
#define kDoSdkCPLogoutNotification @"k_DoSdk_cp_logout_notification"

/** 切换账号结果的通知 */
#define kDoSdkCPSwitchNotification @"k_DoSdk_cp_switch_notification"

/** 离开平台界面时,会发送该通知 */
#define kDoSdkCPLeavePlatformNotification @"k_DoSdk_cp_leave_platform_notification"

/** 购买结果的通知 */
#define kDoSdkCPBuyResultNotification @"k_DoSdk_cp_buy_result_notification"

/** 应用完成的通知 */
#define kDoSdkCPAppVersionUpdateNotification @"k_DoSdk_cp_app_version_update_notification"

/** 支付宝快捷支付结束通知 OpenUrl调用 */
#define kDoSdkPlatformAlixQuickPayEnd @"k_DoSdk_cp_zfb_paysuccess_result"

/** 微信支付结束通知 OpenUrl调用*/
#define kDoSdkPlatformWeChatPayEnd @"k_DoSdk_cp_wechat_paysuccess_result"

/** OpenUrl调用*/
#define kDoSdkApplicationOpenUrlSource @"k_DoSdk_cp_openurl_notification"
