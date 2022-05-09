package com.debug.test;

import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Environment;
import android.util.Log;
import android.widget.Toast;

import com.unity3d.player.UnityPlayerActivity;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public class UnityToolsActivity {
    private Context context = null;
    private UnityPlayerActivity mainActivity;

    public void init(Context context){
        if(this.context == null){
            this.context = context;
            mainActivity = (UnityPlayerActivity)context;
        }
    }

    // 保存图片到相册
    public void savePhoto(final String fileName) {
        mainActivity.runOnUiThread(new Runnable() {
            public void run() {
                Bitmap bitmap = BitmapFactory.decodeFile(Environment
                        .getExternalStorageDirectory()
                        + "/Android/data/" + mainActivity.getPackageName() + "/files/"
                        + fileName);

                File file = new File(Environment.getExternalStorageDirectory()
                        + "/DCIM/Camera", fileName);

                FileOutputStream fos = null;
                try {
                    fos = new FileOutputStream(file);
                } catch (FileNotFoundException e) {
                    // TODO Auto-generated catch block
                    Log.w("cat", e.toString());
                }
                bitmap.compress(Bitmap.CompressFormat.PNG, 100, fos);

                try {
                    fos.flush();
                } catch (IOException e) {
                    // TODO Auto-generated catch block
                    Log.w("cat", e.toString());
                }
                try {
                    fos.close();
                } catch (IOException e) {
                    // TODO Auto-generated catch block
                    Log.w("cat", e.toString());
                }
                bitmap.recycle();//扫描保存的图片
                context.sendBroadcast(new Intent(Intent.ACTION_MEDIA_SCANNER_SCAN_FILE, Uri.parse("file://" +Environment.getExternalStorageDirectory()
                        + "/DCIM/Camera/"+fileName)));
                Toast.makeText(context, "截图已保存到相册", Toast.LENGTH_SHORT).show();
            }
        });
    }
    //获取App发过来的消息
    public String getAppData(String key) { return mainActivity.getIntent().getStringExtra(key); }
    //退出UnityActivity
//    public void quitUnityActivity() { mainActivity.isDestroyed(); }
}
