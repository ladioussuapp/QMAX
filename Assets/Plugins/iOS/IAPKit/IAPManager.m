//
//  IAPManager.m
//  Unity-iPhone
//
//  Created by MacMini on 14-5-16.
//
//

#import "IAPManager.h"
#import "IAPInterface.h"


@implementation IAPManager

//未处理的订单
NSMutableArray * UnhandleTransactions;

-(void) attachObserver{
    NSLog(@"AttachObserver");
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
}


//是否可以购买物品
-(BOOL) CanMakePayment{
    return [SKPaymentQueue canMakePayments];
}

-(void) requestProductData:(NSSet *)productIdentifiers
{
    [self sendRequest:productIdentifiers];
}

-(void)sendRequest:(NSSet *)idSet
{
    SKProductsRequest *request = [[SKProductsRequest alloc] initWithProductIdentifiers:idSet];
    //Keep a strong reference to the request
    request.delegate = self;
    [request start];
}

#pragma mark SKProductsRequestDelegate
//发送请求的回调
-(void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
{
    self->products = response.products;
    [IAPInterface onProductsInfoResponse:response];
}


-(void) finishTransaction:(NSString *)transactionID
{
    NSLog(@"finishTransaction %@", transactionID);
    if(UnhandleTransactions == nil)
        return;
    
    SKPaymentTransaction* targetTransaction = nil;
    for (SKPaymentTransaction *transaction in UnhandleTransactions)
    {
        if([transactionID isEqualToString:transaction.transactionIdentifier])
        {
            targetTransaction = transaction;
            break;
        }
    }
    
    if(targetTransaction != nil)
    {
        [UnhandleTransactions removeObject:targetTransaction];
        [[SKPaymentQueue defaultQueue] finishTransaction:targetTransaction];
    }
}


-(void)buyRequest:(NSString *)productIdentifier quantity:(NSInteger)qtt withUser:(NSString *)appUsername
{
    SKProduct *targetProdcut = nil;
    for(SKProduct *p in products)
    {
        if([p.productIdentifier isEqualToString:productIdentifier])
        {
            targetProdcut = p;
            break;
        }
    }
    
    if(targetProdcut != nil)
    {
        SKMutablePayment *payment = [SKMutablePayment paymentWithProduct:targetProdcut];
        payment.quantity = qtt;
        payment.applicationUsername = appUsername;
        [[SKPaymentQueue defaultQueue] addPayment:payment];
    }
    else
    {
        NSLog(@"Unfind target product, id=%@", productIdentifier);
    }
}



#pragma mark SKPaymentTransactionObserver
-(void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions
{
    UnhandleTransactions = [transactions mutableCopy];
    for (SKPaymentTransaction *transaction in transactions) {
        switch (transaction.transactionState) {
            case SKPaymentTransactionStatePurchased:
                NSLog(@"Comblete transaction : %@",transaction.transactionIdentifier);
                [IAPInterface onPaymentTransactionComplete:transaction];
                //[[SKPaymentQueue defaultQueue] finishTransaction:transaction];
                break;
            case SKPaymentTransactionStateFailed:
                NSLog(@"Failed transaction : %@",transaction.transactionIdentifier);
                [IAPInterface onPaymentTransactionFail:transaction];
                if (transaction.error.code != SKErrorPaymentCancelled) {
                    NSLog(@"!Cancelled");
                }
                //[[SKPaymentQueue defaultQueue] finishTransaction:transaction];
                break;
            case SKPaymentTransactionStateRestored:
                NSLog(@"Restore transaction : %@",transaction.transactionIdentifier);
                [IAPInterface onPaymentTransactionRestore:transaction];
                //[[SKPaymentQueue defaultQueue] finishTransaction:transaction];
                break;
            default:
                break;
        }
    }
}

@end
