package org.mobileforge.samplegame;

import android.app.Activity;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.webkit.ConsoleMessage;
import android.webkit.WebChromeClient;
import android.webkit.WebResourceRequest;
import android.webkit.WebResourceResponse;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import java.io.IOException;

public final class MainActivity extends Activity {
    private WebView game;

    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        game = new WebView(this);
        game.getSettings().setJavaScriptEnabled(true);
        game.getSettings().setAllowFileAccess(false);
        game.getSettings().setAllowContentAccess(false);
        game.setWebChromeClient(new WebChromeClient() {
            @Override public boolean onConsoleMessage(ConsoleMessage message) {
                Log.d("MobileForge", message.message());
                return true;
            }
        });
        game.setWebViewClient(new WebViewClient() {
            @Override public WebResourceResponse shouldInterceptRequest(WebView view, WebResourceRequest request) {
                if (!"https".equals(request.getUrl().getScheme())
                    || !"mobileforge.local".equals(request.getUrl().getHost())
                    || !"/game.html".equals(request.getUrl().getPath())) return null;
                try {
                    return new WebResourceResponse("text/html", "UTF-8", getAssets().open("game.html"));
                } catch (IOException error) {
                    Log.e("MobileForge", "Missing game.html", error);
                    return null;
                }
            }
            @Override public boolean shouldOverrideUrlLoading(WebView view, WebResourceRequest request) {
                return !("mobileforge.local".equals(request.getUrl().getHost())
                    && "/game.html".equals(request.getUrl().getPath()));
            }
        });
        setContentView(game);
        game.loadUrl("https://mobileforge.local/game.html");
        Handler monitor = new Handler(Looper.getMainLooper());
        Runnable check = new Runnable() {
            @Override public void run() {
                game.evaluateJavascript("(function(){const e=document.querySelector('#error'),c=document.querySelector('canvas');if(e&&!e.hidden)return 'error:'+e.textContent;if(c&&Number(c.dataset.frames)>4)return 'ready';return 'loading'})()", result -> {
                    if (result.contains("ready")) Log.i("MobileForge", "GAME_READY");
                    else if (result.contains("error:")) Log.e("MobileForge", "GAME_ERROR " + result);
                    else monitor.postDelayed(this, 3000);
                });
            }
        };
        monitor.postDelayed(check, 3000);
    }

    @Override public void onBackPressed() {
        if (game.canGoBack()) game.goBack(); else super.onBackPressed();
    }

    @Override protected void onDestroy() {
        game.destroy();
        super.onDestroy();
    }
}
