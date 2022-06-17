package com.genimous.linjing;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.provider.MediaStore;
import android.util.Log;
import android.widget.Toast;

import com.unity3d.player.UnityPlayerActivity;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public class UnityTools extends UnityPlayerActivity {
    private static Context mContext = null;
    private static Activity mainActivity = null;

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

    //退出UnityActivity
    public static void quitUnityActivity() { mainActivity.finish(); }
    
    // 保存图片到相册
    public static void savePhoto(String folder, String fileName) {
        mainActivity.runOnUiThread(new Runnable() {
            public void run() {
                String filePath = Environment
                        .getExternalStorageDirectory()
                        + "/Android/data/" + mContext.getPackageName() + "/files/" + folder;
                Bitmap bitmap = BitmapFactory.decodeFile(filePath + "/" + fileName);
                // 首先保存图片
                File file = new File(filePath, fileName);
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
                    MediaStore.Images.Media.insertImage(mContext.getContentResolver(),
                            file.getAbsolutePath(), folder + "/" + fileName, null);
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
}
