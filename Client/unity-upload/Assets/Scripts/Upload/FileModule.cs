using System;
using System.Collections;
using System.IO;
using System.Net.Security;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Upload
{
    //图片文件类型
    public enum ImageFileType
    {
        Unknown,
        JPG,
        PNG,
    }

    //文件组件
    public static class FileModule
    {
        //检查下载文件本地是否存在
        public static bool CheckDownloadFileExist(string fileUrl, string savePath)
        {
            string fileName = GetFileNameFromUrl(fileUrl);
            string filePath = Path.Combine(savePath, fileName);
            if (File.Exists(filePath))
            {
                return true;
            }
            return false;
        }

        //本地下载文件
        public static void DownloadFileByUrlForNative(string fileUrl, string savePath, Action<string, byte[]> callBack = null)
        {
            string fileName = GetFileNameFromUrl(fileUrl);
            string filePath = Path.Combine(savePath, fileName);
            if (File.Exists(filePath))
            {
                if (callBack != null)
                {
                    byte[] fileData = File.ReadAllBytes(filePath);
                    callBack(fileName,fileData);
                }
            }
            else
            {
                if (callBack != null)
                {
                    callBack(fileName,null);
                }
            }
        }

        //根据url下载&保存文件
        public static IEnumerator DownloadFileByUrl(string fileUrl,string savePath,Action<string,byte[]> callBack = null)
        {
            string fileName = GetFileNameFromUrl(fileUrl);

            UnityWebRequest request = UnityWebRequest.Get(fileUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error downloading file: {request.error}");
                if (callBack != null)
                {
                    callBack(fileName,null);
                }
            }
            else
            {
                byte[] fileData = request.downloadHandler.data;
                if (savePath != null)
                {
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    string filePath = Path.Combine(savePath, fileName);
                    File.WriteAllBytes(filePath, fileData);
                    Debug.LogError("write file in " + filePath);
                }

                if (callBack != null)
                {
                    callBack(fileName,fileData);
                }
            }
        }
        
        // 解析 JSON 数据的模型类
        [System.Serializable]
        public class FileUrlResponse
        {
            public string fileURL;
        }
        
        //上传文件
        public static IEnumerator UploadFile(string uploadUrl, byte[] fileData, string fileName, Action<string, string> callBack = null)
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", fileData, fileName);

            // Create a UnityWebRequest with the URL, POST method, and the WWWForm
            UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form);

            // Send the request and wait for the response
            yield return www.SendWebRequest();

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Upload failed: {www.error}");
                if (callBack != null)
                {
                    callBack(fileName,"");
                }
            }
            else
            {
                Debug.Log("Upload complete!");
                // 处理返回的数据
                string responseText = www.downloadHandler.text;
                Debug.Log("Response: " + responseText);

                // 解析 JSON 数据
                FileUrlResponse responseData = JsonUtility.FromJson<FileUrlResponse>(responseText);

                // 检查并使用 fileURL 字段
                if (responseData != null)
                {
                    string fileURL = responseData.fileURL;
                    Debug.Log("File URL: " + fileURL);
                    if (callBack != null)
                    {
                        callBack(fileName,fileURL);
                    }
                }
                else
                {
                    Debug.LogError("Failed to parse JSON response.");
                    if (callBack != null)
                    {
                        callBack(fileName,"");
                    }
                }
            }
        }

        //获取下载目录
        public static string GetDownloadFolderPath()
        {
            string downloadFolderPath = Path.Combine(Application.persistentDataPath, "Download");
            // 确保 Download 文件夹存在
            if (!Directory.Exists(downloadFolderPath))
            {
                Directory.CreateDirectory(downloadFolderPath);
            }
            return downloadFolderPath;
        }
        
        //获取下载文件的文件名
        public static string GetFileNameFromUrl(string url)
        {
            // 解析 URL 并提取文件名
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.AbsolutePath);
            return fileName;
        }

        //获取目录的文件名
        public static string GetFileNameFromPath(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        //根据路径获取texture
        public static Texture2D LoadImageByPath(string filePath)
        {
            // 检查文件是否存在
            if (File.Exists(filePath))
            {
                // 读取文件数据
                byte[] fileData = File.ReadAllBytes(filePath);

                // 创建 Texture2D 对象
                Texture2D texture = new Texture2D(2, 2); // 创建一个初始大小的 Texture2D 对象
                if (texture.LoadImage(fileData)) // 将字节数据加载到 Texture2D
                {
                    return texture;
                }
                else
                {
                    Debug.LogError("Failed to load image data into Texture2D.");
                }
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }

            return null;
        }

        //获取图片文件类型
        public static ImageFileType GetImageFileType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (extension == ".jpg" || extension == ".jpeg")
            {
                return ImageFileType.JPG;
            }
            else if(extension == ".png")
            {
                return ImageFileType.PNG;
            }

            return ImageFileType.Unknown;
        }

        //是否是PNG类型
        public static bool IsImageFileTypePNG(string fileName)
        {
            ImageFileType type = GetImageFileType(fileName);
            if (type == ImageFileType.PNG) return true;
            return false;
        }
        
        //是否是JPG类型
        public static bool IsImageFileTypeJPG(string fileName)
        {
            ImageFileType type = GetImageFileType(fileName);
            if (type == ImageFileType.JPG) return true;
            return false;
        }

        //texture-png转换文件数据
        public static byte[] Texture2PNGFileData(Texture2D texture)
        {
            if (texture != null)
            {
                // 确保纹理是可读的
                if (!texture.isReadable)
                {
                    Debug.LogError("Texture is not readable.");
                    return null;
                }
                
                // 确保纹理的格式可以被正确编码
                Texture2D tex = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
        
                // 复制原纹理的像素数据
                tex.SetPixels(texture.GetPixels());
                tex.Apply();

                // 将纹理转换为PNG格式的字节数组
                byte[] pngData = tex.EncodeToPNG();
                return pngData;
            }
            return null;
        }
        
        //texture-jpg转换文件数据
        public static byte[] Texture2JPGFileData(Texture2D texture)
        {
            if (texture != null)
            {
                // 确保纹理是可读的
                if (!texture.isReadable)
                {
                    Debug.LogError("Texture is not readable.");
                    return null;
                }
                
                // 确保纹理的格式可以被正确编码
                Texture2D tex = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
        
                // 复制原纹理的像素数据
                tex.SetPixels(texture.GetPixels());
                tex.Apply();

                // 将纹理转换为JPG格式的字节数组
                byte[] jpgData = tex.EncodeToJPG();
                return jpgData;
            }
            return null;
        }

        //文件数据转Texture
        public static Texture2D FileData2Texture(byte[] fileData)
        {
            if (fileData != null && fileData.Length > 0)
            {
                // 创建一个新的 Texture2D
                Texture2D texture = new Texture2D(2, 2); // 初始尺寸会被自动调整

                // 加载图像数据到 Texture2D 中
                if (texture.LoadImage(fileData))
                {
                    return texture;
                }
                else
                {
                    Debug.LogError("Failed to load image data into Texture2D.");
                }
            }

            return null;
        }

        //获取文件数据
        public static byte[] GetFileData(string fileName,RawImage targetRawImage)
        {
            byte[] fileData = null;
            if (targetRawImage == null)
            {
                return fileData;
            }

            if (targetRawImage.texture is Texture2D texture2D)
            {
                if (FileModule.IsImageFileTypeJPG(fileName))
                {
                    if (texture2D != null)
                    {
                        fileData = FileModule.Texture2JPGFileData(texture2D);
                    }
                }
                else if (FileModule.IsImageFileTypePNG(fileName))
                {
                    if (texture2D != null)
                    {
                        fileData = FileModule.Texture2PNGFileData(texture2D);
                    }
                }
            }
            else if (targetRawImage.texture is RenderTexture renderTexture)
            {
                // 处理 RenderTexture
                Texture2D tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
    
                // 将 RenderTexture 的内容读入 Texture2D
                RenderTexture.active = renderTexture;
                tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                tempTexture.Apply();
                RenderTexture.active = null;

                if (tempTexture != null)
                {
                    if (FileModule.IsImageFileTypeJPG(fileName))
                    {
                        if (tempTexture != null)
                        {
                            fileData = FileModule.Texture2JPGFileData(tempTexture);
                        }
                    }
                    else if (FileModule.IsImageFileTypePNG(fileName))
                    {
                        if (tempTexture != null)
                        {
                            fileData = FileModule.Texture2PNGFileData(tempTexture);
                        }
                    }
                }
            }

            return fileData;
        }
        
        //深度拷贝Texture
        public static Texture2D DeepCopyTexture(Texture2D sourceTexture)
        {
            // 创建一个与源纹理尺寸相同的 RenderTexture
            RenderTexture tempRenderTexture =
                new RenderTexture(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture.active = tempRenderTexture;

            // 将源纹理渲染到 RenderTexture 中
            Graphics.Blit(sourceTexture, tempRenderTexture);

            // 将 RenderTexture 的内容读取到目标纹理
            Texture2D destinationTexture =
                new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
            destinationTexture.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);
            destinationTexture.Apply();

            // 释放资源
            RenderTexture.active = null;
            tempRenderTexture.Release();
            GameObject.Destroy(tempRenderTexture);
            return destinationTexture;
        }
    }
}