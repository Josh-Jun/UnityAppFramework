package com.debug.test;

import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.util.Log;
import android.widget.Toast;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public class UnityMainActivity extends UnityPlayerActivity {
    public Context context = null;
    public UnityMainActivity main = null;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if(main == null){ main = this; }
        if(context == null){ context = main.getApplicationContext(); }
    }
    // 保存图片到相册
    public void savePhoto(final String fileName) {
        main.runOnUiThread(new Runnable() {
            public void run() {
                Bitmap bitmap = BitmapFactory.decodeFile(Environment
                        .getExternalStorageDirectory()
                        + "/Android/data/" + main.getPackageName() + "/files/"
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
    public String getAppData(String key) { return main.getIntent().getStringExtra(key); }
    //退出UnityActivity
    public void quitUnityActivity() { main.mUnityPlayer.quit(); }
}
