package com.loves.qmax;

import java.io.File;

import android.app.Activity;
import android.os.Bundle;

import com.yasdksdemo.MainActivity;
//bugly code import mark.
//XGPush code import mark.

public class QMaxActivity extends MainActivity {

	private static Activity _gameContext;
	
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		_gameContext = this;
		//bugly code initCrashReport mark.
	}
	
	//XGPush code registerPush mark.

	public static String getQMaxFilesPath() {
		boolean hasExternal = android.os.Environment.getExternalStorageState().equals(android.os.Environment.MEDIA_MOUNTED);
		String qmaxRoot = "";
		if (hasExternal) {
			File sdDir = android.os.Environment.getExternalStorageDirectory();
			File qmax = new File(sdDir.getAbsolutePath() + "/qmax");
			if (qmax.exists()) {
				qmaxRoot = qmax.getAbsolutePath();
			} else if (qmax.mkdirs()) {
				qmaxRoot = qmax.getAbsolutePath();
			}
		}
		if (qmaxRoot.equals("")) {
			qmaxRoot = _gameContext.getFilesDir().getAbsolutePath();
		}
		return qmaxRoot;
	}
	
}
