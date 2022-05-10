package com.genimous.linjing;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Context;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.os.FileUtils;
import android.provider.MediaStore;
import android.widget.Toast;

import com.unity3d.player.UnityPlayerActivity;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.net.FileNameMap;
import java.net.URLConnection;

public class UnityTools extends UnityPlayerActivity {
    private static Context mContext = null;
    private static Activity _mainActivity;

    public static void init(Context context) {
        if (mContext == null) {
            mContext = context;
        }
    }

    // 获取App发过来的消息
    public static String getAppData(String key) {
        return getMainActivity().getIntent().getStringExtra(key);
    }

    // 保存图片到相册
    public static void savePhoto(final String fileName) {
        getMainActivity().runOnUiThread(new Runnable() {
            public void run() {
                File file = new File(Environment
                        .getExternalStorageDirectory()
                        + "/Android/data/" + mContext.getPackageName() + "/files/Screenshot"
                        , fileName);
                String mimeType = getMimeType(file);
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
                    String name = file.getName();
                    ContentValues values = new ContentValues();
                    values.put(MediaStore.MediaColumns.DISPLAY_NAME, name);
                    values.put(MediaStore.MediaColumns.MIME_TYPE, mimeType);
                    values.put(MediaStore.MediaColumns.RELATIVE_PATH, Environment.DIRECTORY_DCIM + "/Meta");
                    ContentResolver contentResolver = mContext.getContentResolver();
                    Uri uri = contentResolver.insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, values);
                    if (uri == null) {
                        Toast.makeText(mContext, "截图保存失败", Toast.LENGTH_SHORT).show();
                        return;
                    }
                    try {
                        OutputStream out = contentResolver.openOutputStream(uri);
                        FileInputStream fis = new FileInputStream(file);
                        FileUtils.copy(fis, out);
                        fis.close();
                        out.close();
                        Toast.makeText(mContext, "截图保存成功", Toast.LENGTH_SHORT).show();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                } else {
                    MediaScannerConnection.scanFile(mContext, new String[]{file.getPath()}, new String[]{mimeType}, (path, uri) -> {
                        Toast.makeText(mContext, "截图已保存到相册", Toast.LENGTH_SHORT).show();
                    });
                }
            }
        });
    }

    private static String getMimeType(File file) {
        FileNameMap fileNameMap = URLConnection.getFileNameMap();
        String type = fileNameMap.getContentTypeFor(file.getName());
        return type;
    }

    //获取unity项目的Activity
    private static Activity getMainActivity() {
        if (null == _mainActivity) {
            try {
                Class<?> classmate = Class.forName("com.unity3d.player.UnityPlayer");
                Activity activity = (Activity) classmate.getDeclaredField("currentActivity").get(classmate);
                _mainActivity = activity;
            } catch (ClassNotFoundException e) {

            } catch (IllegalAccessException e) {

            } catch (NoSuchFieldException e) {

            }
        }
        return _mainActivity;
    }
}
