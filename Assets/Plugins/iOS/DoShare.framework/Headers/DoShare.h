//
//  DoShare.h
//  DoShare
//
//  Created by zhai chunlin on 15/11/11.
//  Copyright (c) 2015年 YA. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef enum : NSUInteger {
    DoWXShare,
    DoFacebookShare,
    DoLineShare,
} DoShareType;

#if defined(__cplusplus)
extern "C"{
#endif
    //分享结果回调函数类型声明
    typedef void (*DoShareCallBack)(DoShareType shareType, int resultStatus);
    
    //分享
    extern void iosDoShare(DoShareType shareType, const char *appID, const char *title, const char *description, const char *imagePath);
    
    //注册回调函数
    extern void iosSetShareListener(DoShareCallBack callback);
#if defined(__cplusplus)
}
#endif