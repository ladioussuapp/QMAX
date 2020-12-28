//
//  UJSInterface.m
//  Unity-iPhone
//
//  Created by MacMini on 14-5-15.
//
//

#import "IAPInterface.h"
#import "IAPManager.h"

@implementation IAPInterface

//调用的Unity
NSString *unityGOName = nil;

IAPManager *iapManager = nil;

#pragma mark Unity Invoke

float TestMsg(){
    NSLog(@"Msg received");
    return 0.71f;
}

void TestSendString(void *p){
    NSString *list = [NSString stringWithUTF8String:p];
    NSArray *listItems = [list componentsSeparatedByString:@"\t"];
    
    for (int i =0; i<listItems.count; i++) {
        NSLog(@"msg %d : %@",i,listItems[i]);
    }
}

void TestGetString(){
    NSArray *test = [NSArray arrayWithObjects:@"t1",@"t2",@"t3", nil];
    NSString *join = [test componentsJoinedByString:@"\n"];
    UnitySendMessage(unityGOName.UTF8String, "IOSToU", [join UTF8String]);
}

#pragma mark

//初始化SDK
void Init(void *goName){
    unityGOName = [NSString stringWithUTF8String:goName];
    iapManager = [[IAPManager alloc] init];
    [iapManager attachObserver];
}

//是否可以购买
bool CanMakePayment(){
    return [iapManager CanMakePayment];
}

//请求商品信息
void RequstProductInfo(void *p){
    NSString *list = [NSString stringWithUTF8String:p];
    NSArray *idArray = [list componentsSeparatedByString:@"\t"];
    NSSet *idSet = [NSSet setWithArray:idArray];
    [iapManager requestProductData:idSet];
}

//购买物品
void BuyProduct(void *p){
    NSString *list = [NSString stringWithUTF8String:p];
    NSArray *idArray = [list componentsSeparatedByString:@"\t"];
    NSString* productID = idArray[0];
    NSInteger qtt = [idArray[1] integerValue];
    NSString* appUsername = idArray[2];
    [iapManager buyRequest:productID quantity:qtt withUser:appUsername];
}

//完成一个订单处理
void FinishTransaction(void *p)
{
    NSString* transactionID = [NSString stringWithUTF8String:p];
    [iapManager finishTransaction:transactionID];
}


#pragma mark

//商品信息返回
+(void) onProductsInfoResponse:(SKProductsResponse*)response
{
    const char* str = [[IAPInterface encodeProductsInfoResponse:response] UTF8String];
    UnitySendMessage(unityGOName.UTF8String, "onProductsInfoResponse", str);
}

//将SKProductsResponse编码
+(NSString*) encodeProductsInfoResponse:(SKProductsResponse*)response
{
    NSInteger count = response.products.count;
    //count += response.invalidProductIdentifiers.count;
    
    NSMutableArray* array = [NSMutableArray arrayWithCapacity:count];
    for (SKProduct *p in response.products)
    {
        //NSLog(@"--------- %@ %@", p.localizedTitle, p.price);
        NSDictionary *dict = [NSDictionary dictionaryWithObjectsAndKeys:
                              p.productIdentifier, @"productIdentifier",
                              [p.price stringValue], @"price",
                              p.localizedTitle, @"localizedTitle",
                              p.localizedDescription, @"localizedDescription",
                              //@"product.priceLocale", @"priceLocale",
                              nil];
        //只返回有效的物品的信息
        [array addObject:dict];
    }
    
    for(NSString *invalidProductId in response.invalidProductIdentifiers)
    {
        NSLog(@"Invalid product id:%@",invalidProductId);
    }
    
    
    NSError *error;
    NSData* data = [NSJSONSerialization dataWithJSONObject:array options:nil error:&error];
    NSString *ret = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    NSLog(@"product info = %@", ret);
    
    return ret;
}

#pragma mark
//支付完成
+(void) onPaymentTransactionComplete:(SKPaymentTransaction *)transaction
{
    NSString* transactionID = transaction.transactionIdentifier;
    NSString* appUsername = [[transaction payment] applicationUsername];
    NSString* receipt = [IAPInterface encodeTransactionReceipt:transaction];
    NSArray * retList = [NSArray arrayWithObjects: transactionID, appUsername, receipt, nil];
    NSString* ret = [retList componentsJoinedByString:@"\t"];
    
    UnitySendMessage(unityGOName.UTF8String,
                     "onPaymentTransactionComplete",
                     [ret UTF8String]);
}

//支付失败
+(void) onPaymentTransactionFail:(SKPaymentTransaction *)transaction
{
    NSString* transactionID = transaction.transactionIdentifier;
    NSString* appUsername = [[transaction payment] applicationUsername];
    NSArray * retList = [NSArray arrayWithObjects: transactionID, appUsername, nil];
    NSString* ret = [retList componentsJoinedByString:@"\t"];
    UnitySendMessage(unityGOName.UTF8String, "onPaymentTransactionFail", ret.UTF8String);
}

//支付恢复
+(void) onPaymentTransactionRestore:(SKPaymentTransaction *)transaction
{
    NSString* transactionID = transaction.transactionIdentifier;
    NSString* appUsername = [[transaction payment] applicationUsername];
    NSArray * retList = [NSArray arrayWithObjects: transactionID, appUsername, nil];
    NSString* ret = [retList componentsJoinedByString:@"\t"];
    UnitySendMessage(unityGOName.UTF8String, "onPaymentTransactionRestore", ret.UTF8String);
}



//将TransactionReceipt编码
+(NSString *)encodeTransactionReceipt:(SKPaymentTransaction *)transaction
{
    return [IAPInterface base64Encoding:(uint8_t *)transaction.transactionReceipt.bytes length:transaction.transactionReceipt.length];
    
    //return [[NSString alloc] initWithData:transaction.transactionReceipt encoding:NSASCIIStringEncoding];
}


+(NSString *)base64Encoding:(const uint8_t *)input length:(NSInteger) length{
    static char table[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    
    NSMutableData *data = [NSMutableData dataWithLength:((length+2)/3)*4];
    uint8_t *output = (uint8_t *)data.mutableBytes;
    
    for(NSInteger i=0; i<length; i+=3){
        NSInteger value = 0;
        for (NSInteger j= i; j<(i+3); j++) {
            value<<=8;
            
            if(j<length){
                value |=(0xff & input[j]);
            }
        }
        
        NSInteger index = (i/3)*4;
        output[index + 0] = table[(value>>18) & 0x3f];
        output[index + 1] = table[(value>>12) & 0x3f];
        output[index + 2] = (i+1)<length ? table[(value>>6) & 0x3f] : '=';
        output[index + 3] = (i+2)<length ? table[(value>>0) & 0x3f] : '=';
    }
    
    return [[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding];
}


//将Product编码为json
+(NSString *)productInfo:(SKProduct *)product
{
    NSLog(@"product.localizedTitle = %@", product.localizedTitle);
    NSLog(@"product.price = %@", product.price);
    
    NSArray *info = [NSArray arrayWithObjects:product.localizedTitle,product.localizedDescription,product.price,product.productIdentifier, nil];
    return [info componentsJoinedByString:@"\t"];
    
    //    NSDictionary *dict = [NSDictionary dictionaryWithObjectsAndKeys:
    //                          product.productIdentifier, @"productIdentifier",
    //                          product.localizedTitle, @"localizedTitle",
    //                          product.localizedDescription, @"localizedDescription",
    //                          [product.price stringValue], @"price",
    //                          //@"product.priceLocale", @"priceLocale",
    //                          nil];
    //    NSError *error;
    //    NSData* data = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:&error];
    //    NSString *ret = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    //    NSLog(@"product info = %@", ret);
    //    return ret;
}

@end
