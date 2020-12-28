package com.loves.qmax;

import java.io.File;
import java.io.FileOutputStream;
import java.util.HashMap;
import java.util.List;

import org.json.JSONException;
import org.json.JSONObject;
import org.json.JSONTokener;

import android.annotation.SuppressLint;
import android.app.ActivityManager;
import android.app.ActivityManager.RunningTaskInfo;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.util.Log;
import android.widget.RemoteViews;

import com.tencent.android.tpush.XGPushBaseReceiver;
import com.tencent.android.tpush.XGPushClickedResult;
import com.tencent.android.tpush.XGPushRegisterResult;
import com.tencent.android.tpush.XGPushShowedResult;
import com.tencent.android.tpush.XGPushTextMessage;

public class QMaxPushReceiver extends XGPushBaseReceiver {

	private static HashMap<String, Integer> icons = new HashMap<String, Integer>();

	static {
		icons.put("app_icon.png", R.drawable.app_icon);
	}

	@SuppressLint("SimpleDateFormat")
	@Override
	public void onTextMessage(Context context, XGPushTextMessage message) {
		String qmaxRoot = getPushFilesPath(context);
		File pushswitch = new File(qmaxRoot + "/pushswitch.dat");
		if (pushswitch.exists()) {
			// close push
			Log.i("XGPushManager", "XGPush setting close push...");
			return;
		}

		String title = message.getTitle();
		String content = message.getContent();
		ActivityManager manager = (ActivityManager) context.getSystemService(Context.ACTIVITY_SERVICE);

		ApplicationInfo appInf = context.getApplicationInfo();
		String selfPackageName = appInf.packageName;

		boolean isRuning = false;
		List<RunningTaskInfo> runningTaskInfos = manager.getRunningTasks(1);
		for (int i = 0; i < runningTaskInfos.size(); i++) {
			RunningTaskInfo inf = runningTaskInfos.get(i);
			if (inf == null) {
				continue;
			}
			Log.i("XGPushManager", "XGPush running task : [" + inf.topActivity.getPackageName() + "]");
			if (inf.topActivity.getPackageName().contains(selfPackageName)) {
				isRuning = true;
				break;
			}
		}
		if (!isRuning) {
			String iconName = "";
			String data = "";
			if (content.contains("{") && content.contains("}")) {
				try {
					JSONTokener jsonParser = new JSONTokener(content);
					JSONObject person = (JSONObject) jsonParser.nextValue();
					if (person.has("title")) {
						title = person.getString("title");
					}
					if (person.has("content")) {
						content = person.getString("content");
					}
					if (person.has("icon")) {
						iconName = person.getString("icon");
					}
					if (person.has("data")) {
						data = person.get("data").toString();
					}
				} catch (JSONException e) {
					e.printStackTrace();
				}
			}
			int icon = R.drawable.app_icon;
			if (icons.get(iconName) != null) {
				icon = icons.get(iconName);
			}

			String ns = Context.NOTIFICATION_SERVICE;
			NotificationManager mNotificationManager = (NotificationManager) context.getSystemService(ns);

			Notification notification = new Notification();
			notification.icon = icon;
			notification.tickerText = appInf.name;
			notification.defaults |= Notification.DEFAULT_SOUND;
			notification.defaults |= Notification.DEFAULT_VIBRATE;
			notification.flags = Notification.FLAG_AUTO_CANCEL;

			RemoteViews contentView = new RemoteViews(context.getPackageName(), R.layout.notify_view);
			contentView.setImageViewResource(R.id.notify_image, icon);
			contentView.setTextViewText(R.id.notify_title, title);
			contentView.setTextViewText(R.id.notify_content, content);
			notification.contentView = contentView;

			Intent notificationIntent = new Intent(context, QMaxActivity.class);
			// App若在运行则直接拉起Activity
			notificationIntent.setAction(Intent.ACTION_MAIN);
			notificationIntent.addCategory(Intent.CATEGORY_LAUNCHER);
			notificationIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_RESET_TASK_IF_NEEDED);

			PendingIntent contentIntent = PendingIntent.getActivity(context, 0, notificationIntent, 0);
			notification.contentIntent = contentIntent;
			mNotificationManager.notify(1, notification);

			// 保存推送数据
			if (data != null && !data.equals("")) {
				File out = new File(qmaxRoot + "/push.dat");
				try {
					FileOutputStream fos = new FileOutputStream(out);
					byte[] bytes = data.getBytes("UTF-8");
					fos.write(bytes);
					fos.flush();
					fos.close();
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
			Log.i("XGPushManager", "XGPush onTextMessage title : " + title + " | content: " + content + " | data: " + data);
		}
	}

	private String getPushFilesPath(Context context) {
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
			qmaxRoot = context.getFilesDir().getAbsolutePath();
		}
		return qmaxRoot;
	}

	@Override
	public void onDeleteTagResult(Context arg0, int arg1, String arg2) {
		// TODO Auto-generated method stub

	}

	@Override
	public void onNotifactionClickedResult(Context arg0, XGPushClickedResult arg1) {
		// TODO Auto-generated method stub

	}

	@Override
	public void onNotifactionShowedResult(Context arg0, XGPushShowedResult arg1) {
		// TODO Auto-generated method stub

	}

	@Override
	public void onRegisterResult(Context arg0, int arg1, XGPushRegisterResult arg2) {
		// TODO Auto-generated method stub

	}

	@Override
	public void onSetTagResult(Context arg0, int arg1, String arg2) {
		// TODO Auto-generated method stub

	}

	@Override
	public void onUnregisterResult(Context arg0, int arg1) {
		// TODO Auto-generated method stub

	}

}
