using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//AzureKinectSDKの読み込み
using Microsoft.Azure.Kinect.Sensor;
//非同期処理をする準備
using System.Threading.Tasks;
public class KinectScript : MonoBehaviour
{
    //Kinectを扱う変数
    Device kinect;
    //表示するPointCloudの全点数
    int num;
    //点の集合を描画するために使用(表示する図形の詳細を管理するオブジェクト)
    Mesh mesh;
    //PointCloudの各点の座標の配列
    Vector3[] vertices;
    //PointCloudの各点に対応する色の配列
    Color32[] colors;
    //vertices中の何番目の点を描画するかのリスト(全部描画するけど手続き上必要)
    int[] indices;
    //座標変換(Color⇔Depth対応やDepth→xyzなど)をするためのクラス
    Transformation transformation;

    void Start()
    {
        //最初の一回だけKinect初期化メソッドを呼び出す
        InitKinect();
        //点群描画のための初期化
        InitMesh();
        //Kinectからのデータ取得と描画(ループ)
        Task t = KinectLoop();
    }

    //Kinectの初期化
    private void InitKinect()
    {
        //0番目のKinectと接続
        kinect = Device.Open(0);
        //Kinectの各種モードを設定して動作開始
        kinect.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        });
        transformation = kinect.GetCalibration().CreateTransformation();
    }

    //Meshを用いてPointCloudを描画する準備をする
    private void InitMesh()
    {
        //Depth画像の横幅と縦幅を取得し、全点数を算出
        int width = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        int height = kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
        num = width * height;

        //meshをインスタンス化
        mesh = new Mesh();
        //65535点以上を描画するため下記を記述
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        //Depth画像の総ピクセル数分の頂点や色の記憶領域を確保
        vertices = new Vector3[num];
        colors = new Color32[num];
        indices = new int[num];

        //描画する点の配列番号を記録。(全ての点を描画)
        for (int i = 0; i < num; i++)
        {
            indices[i] = i;
        }
        //点の座標や色、描画する点のリストをmeshに渡す
        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        //メッシュをこのスクリプトが貼られているオブジェクトのMashFilterに適用
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
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
                Image colorImage = transformation.ColorImageToDepthCamera(capture);
                //色情報のみの配列を取得
                BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();

                //capture.DepthでDepth画像を取得
                //さらにDepthImageToPointCloudでxyzに変換
                Image xyzImage = transformation.DepthImageToPointCloud(capture.Depth);
                //変換後のデータから点の座標のみの配列を取得
                Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();

                //Kinectで取得した全点の座標や色を代入
                for (int i = 0; i < num; i++)
                {
                    //頂点座標の代入
                    vertices[i].x = xyzArray[i].X * 0.001f;
                    vertices[i].y = -xyzArray[i].Y * 0.001f;//上下反転
                    vertices[i].z = xyzArray[i].Z * 0.001f;
                    //色の代入
                    colors[i].b = colorArray[i].B;
                    colors[i].g = colorArray[i].G;
                    colors[i].r = colorArray[i].R;
                    colors[i].a = 255;
                }
                //meshに最新の点の座標と色を渡す
                mesh.vertices = vertices;
                mesh.colors32 = colors;
                mesh.RecalculateBounds();
            }
        }
    }

    //このオブジェクトが消える(アプリ終了)と同時にKinectを停止
    private void OnDestroy()
    {
        kinect.StopCameras();
    }

    void Update()
    {

    }
}