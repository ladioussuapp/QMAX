package com.yasdksdemo;

import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;

import com.baidu.gamesdk.BDGameApplication;
import com.youai.sdks.PlatformSdk;

public class PlatformSDKApplication extends BDGameApplication {
	
	public void initWithManifest() {
		ApplicationInfo appInfo;
		try {
			appInfo = this.getPackageManager().getApplicationInfo(
					this.getPackageName(), PackageManager.GET_META_DATA);
			int plat = appInfo.metaData.getInt("YA_PLATFORM");
			MainActivity.platformInfo = GameMaincpp.getPlatformInfoByType(plat);
		} catch (NameNotFoundException e) {
			e.printStackTrace();
		}
	}
	
	@Override
	protected void attachBaseContext(Context base) {
		super.attachBaseContext(base);
		initWithManifest();
		PlatformSdk.getInstance().attachBaseContext(base, MainActivity.platformInfo);
	}
	
	
	@Override
	public void onCreate() {
		PlatformSdk.getInstance().onApplicationCreate(this, MainActivity.platformInfo);
		super.onCreate();
	}
}
