package com.genimous.linjing;

import android.os.Bundle;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class UnityMainActivity extends UnityPlayerActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
    }
    //退出UnityActivity
    public void quitUnityActivity() { this.mUnityPlayer.quit(); }
    //获取App发过来的消息
    public String getAppData(String key) { return this.getIntent().getStringExtra(key); }
}