//
//  DoSDKError.h
//  DoSDK
//
//  Created by finn on 15/6/17.
//  Copyright (c) 2015年 finn. All rights reserved.
//

#define DoSdk_NO_ERROR                                        0       // 没有错误
#define DoSdk_ERROR_UNKNOWN                                   -1      // 未知错误
#define DoSdk_ERROR_PARAM                                     -3      // 参数无效
#define DoSdk_ERROR_SIGNATURE                                 -4      // 签名无效
#define DoSdk_ERROR_NETWORK                                   -6      // 服务器处理发生错误,请求无法完成
#define DoSdk_ERROR_USER_CANCEL                               -12     // 用户取消
#define DoSdk_ERROR_PLATFORM                                  -102    // 平台无效
#define DoSdk_ERROR_BALANCE_NOT_ENOUGH                        -4002   // 余额不足,无法支付
#define DoSdk_ERROR_ORDER_EMPTY                               -4005   // 订单号为空
