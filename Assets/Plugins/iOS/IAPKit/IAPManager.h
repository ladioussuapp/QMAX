//
//  IAPManager.h
//  Unity-iPhone
//
//  Created by MacMini on 14-5-16.
//
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

@interface IAPManager : NSObject<SKProductsRequestDelegate, SKPaymentTransactionObserver>{
    NSArray<SKProduct *> *products;
    SKProductsRequest *productsRequest;
}

-(void) attachObserver;
-(BOOL) CanMakePayment;
-(void) requestProductData:(NSSet *)productIdentifiers;
-(void) buyRequest:(NSString *)productIdentifier quantity:(NSInteger)qtt withUser:(NSString *)appUsername;

//处理完一个Transaction
-(void) finishTransaction:(NSString *)transactionID;

@end
