//
//  DoSDK+PayAndRecharge.h
//  DoSDK
//
//  Created by finn on 15/6/17.
//  Copyright (c) 2015年 finn. All rights reserved.
//

#import "DoSdkBase.h"

@interface DoSdk (PayAndRecharge)

/**
  @brief 进入游戏币支付界面
  @param 字典字段  productName  NSString  产品名称
  @param 字典字段  order NSString 合作商订单号, 必须保证唯一, 双方对账的唯一标记
  @param 字典字段  amount int 需要支付的金额, 单位分
  @param 字典字段  paydes NSString 支付描述,原样返回
  @param 字典字段  productId NSString  产品id
 */
- (void)pay:(NSDictionary *)dict;

@end
