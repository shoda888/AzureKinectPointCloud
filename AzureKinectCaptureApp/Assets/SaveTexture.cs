using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//(追加1)AzureKinectSDKの読み込み
using Microsoft.Azure.Kinect.Sensor;
using System.Threading.Tasks;
using Image = Microsoft.Azure.Kinect.Sensor.Image;
public class SaveTexture : MonoBehaviour
{
    //(追加2)Kinectを扱う変数
    Device kinect;
    Bitmap colorBitmap;
    // Bitmap depthBitmap;
    //Kinectからのデータ取得と描画(ループ)
    Task t = KinectLoop();

    void Start()
    {
        //(追加5)最初の一回だけKinect初期化メソッドを呼び出す
        InitKinect();
    }
    //(追加3)Kinectの初期化(Form1コンストラクタから呼び出す)
    private void InitKinect()
    {

        //(追加4)0番目のKinectと接続したのちにKinectの各種モードを設定して動作開始
        kinect = Device.Open(0);
        kinect.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        });
    }
    //(追加6)このオブジェクトが消える(アプリ終了)と同時にKinectを停止
    private void OnDestroy()
    {
        kinect.StopCameras();
    }
    //Kinectからデータを取得し、描画するメソッド
    private async Task KinectLoop()
    {
        //while文でkinectからデータを取り続ける
        while (true)
        {
            //GetCaptureでKinectのデータを取得
            using (Capture capture = await Task.Run(() => kinect.GetCapture()).ConfigureAwait(true))
            {
                //Depth画像との位置・サイズ合わせ済みの画像を取得
                // Image colorImage = transformation.ColorImageToDepthCamera(capture);
                Image colorImage = capture.Color;

                //色情報のみの配列を取得
                // BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();
                using (MemoryHandle pin = colorImage.Memory.Pin())
                {
                    //Bitmap画像を作成
                    colorBitmap = new Bitmap(
                            colorImage.WidthPixels, //カラー画像の横幅
                            colorImage.HeightPixels,//カラー画像の縦幅
                            colorImage.StrideBytes, //横一列のバイト数(width*4)
                            PixelFormat.Format32bppArgb,//カラーフォーマット(RGBA)
                            (IntPtr)pin.Pointer); //各ピクセルの色情報
                }
                 
            }
        }
    }
    void Update()
    {
        this.depthBitmap.Save(@"C:\Users\gekka\OneDrive\デスクトップ\temp\test111.png", System.Drawing.Imaging.ImageFormat.Png);
    }
    // public void ButtonClick()
    // {
    //     //Console.WriteLine("Hello");
    //     this.depthBitmap.Save(@"C:\Users\gekka\OneDrive\デスクトップ\temp\test111.png", System.Drawing.Imaging.ImageFormat.Png);
    // }
}