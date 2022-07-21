package com.debug.tools;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.provider.MediaStore;
import android.util.Log;
import android.widget.Toast;

import androidx.core.content.FileProvider;

import com.unity3d.player.UnityPlayerActivity;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.List;

public class AndroidHelper extends UnityPlayerActivity {
    private static Context mContext = null;
    private static Activity mainActivity = null;

    // 初始化
    public static void init(Context context) {
        if (mContext == null) {
            mContext = context;
        }
        if (mainActivity == null) {
            mainActivity = (Activity) context;
        }
    }

    // 获取App发过来的消息
    public static String getAppData(String key) {
        return mainActivity.getIntent().getStringExtra(key);
    }
    // 退出UnityActivity
    public static void quitUnityActivity() { mainActivity.finish(); }
    // 保存图片到相册
    public static void savePhoto(String imagePath) {
        mainActivity.runOnUiThread(new Runnable() {
            public void run() {
                Bitmap bitmap = BitmapFactory.decodeFile(imagePath);
                // 首先保存图片
                File file = new File(imagePath);
                try {
                    FileOutputStream fos = new FileOutputStream(file);
                    bitmap.compress(Bitmap.CompressFormat.PNG, 100, fos);
                    fos.flush();
                    fos.close();
                } catch (FileNotFoundException e) {
                    e.printStackTrace();
                } catch (IOException e) {
                    e.printStackTrace();
                }
                bitmap.recycle();//扫描保存的图片
                // 其次把文件插入到系统图库
                try {
                    Log.i("Unity", file.getAbsolutePath());
                    MediaStore.Images.Media.insertImage(mContext.getContentResolver(),
                            file.getAbsolutePath(), file.getName(), null);
                } catch (FileNotFoundException e) {
                    e.printStackTrace();
                }
                // 最后通知图库更新
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) { // 判断SDK版本是否是4.4或者高于4.4
                    String[] paths = new String[]{file.getAbsolutePath()};
                    MediaScannerConnection.scanFile(mContext, paths, null, null);
                } else {
                    final Intent intent;
                    if (file.isDirectory()) {
                        intent = new Intent(Intent.ACTION_MEDIA_MOUNTED);
                        intent.setClassName("com.android.providers.media", "com.android.providers.media.MediaScannerReceiver");
                        intent.setData(Uri.fromFile(Environment.getExternalStorageDirectory()));
                    } else {
                        intent = new Intent(Intent.ACTION_MEDIA_SCANNER_SCAN_FILE);
                        intent.setData(Uri.fromFile(file));
                    }
                    mContext.sendBroadcast(intent);
                }
                Toast.makeText(mContext, "截图已保存到相册", Toast.LENGTH_SHORT).show();
            }
        });
    }
    // 安装apk
    public static void installApp(String appFullPath) {
        try {
            Intent intent = new Intent(Intent.ACTION_VIEW);
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);

            File apkFile = new File(appFullPath);
            Uri uri = null;
            if (Build.VERSION.SDK_INT >= 24) {
                uri = FileProvider.getUriForFile(mContext, "com.debug.tools.fileProvider", apkFile);
                intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            } else {
                uri = Uri.fromFile(apkFile);
            }

            intent.setDataAndType(uri, "application/vnd.android.package-archive");
            //解决安卓8.0安装界面不弹出
            //查询所有符合 intent 跳转目标应用类型的应用，注意此方法必须放置在 setDataAndType 方法之后
            List<ResolveInfo> resolveLists = mContext.getPackageManager().queryIntentActivities(intent, PackageManager.MATCH_DEFAULT_ONLY);
            // 然后全部授权
            for (ResolveInfo resolveInfo : resolveLists) {
                String packageName = resolveInfo.activityInfo.packageName;
                mContext.grantUriPermission(packageName, uri, Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_WRITE_URI_PERMISSION);
            }
            mContext.startActivity(intent);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}