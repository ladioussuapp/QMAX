//
//  UJSInterface.h
//  Unity-iPhone
//
//  Created by MacMini on 14-5-15.
//
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>


@interface IAPInterface : NSObject

+(void) onProductsInfoResponse:(SKProductsResponse*)response;

+(void) onPaymentTransactionComplete:(SKPaymentTransaction *)transaction;
+(void) onPaymentTransactionFail:(SKPaymentTransaction *)transaction;
+(void) onPaymentTransactionRestore:(SKPaymentTransaction *)transactio;

@end