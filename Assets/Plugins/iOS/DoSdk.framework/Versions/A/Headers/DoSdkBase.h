//
//  DoSDKBase.h
//  DoSDK
//
//  Created by finn on 15/6/17.
//  Copyright (c) 2015年 finn. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

/**
 * 初始化SDK类
 */
@interface DoInitConfigure : NSObject

/** 开发者后台为应用分配的id */
@property (nonatomic, assign) int appId;

/** 开发者后台为应用分配的key */
@property (nonatomic, retain) NSString *appKey;

@end

@interface DoSdk : NSObject

/**
 * 获取DoSdk的实例对象
 */
+ (DoSdk *)defaultSdk;

/**
 * 获取DoSdk版本号
 */
- (NSString *)version;

/**
 * 应用初始化,初始化完成后会发送kDoSdkCPInitDidFinishNotification
 * @param configure 初始化配置类
 */
- (void)init:(DoInitConfigure *)configure;

/**
 * 得到渠道标识
 */
- (NSString *)channel;

/**
 * 显示浮动工具条
 */
- (void)showFloatButton;

/**
 * 隐藏浮动工具条
 */
- (void)hideFloatButton;


/**
 *  得到广告标识
 */
- (NSString *)ad;

/**
 *  设置支付环境：0为沙箱环境，1为正式环境，若不调用此方法，默认正式环境
 */
- (void)setDebug:(int)n;


/**
 @brief 扩展参数(必接)
 @param 字典字段  roleId    NSString    角色id
 @param 字典字段  roleName  NSString    角色名称
 @param 字典字段  roleLevel NSString    角色等级
 @param 字典字段  zoneId    NSString    区服id
 @param 字典字段  zoneName  NSString    区服名称
 @param 字典字段  vipLvl    NSString    vip等级
 @param 字典字段  puid      NSString    平台用户唯一标识
 */
- (void)extendData:(NSDictionary *)dict;

@end

