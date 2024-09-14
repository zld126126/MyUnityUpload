using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Upload
{
    public class UploadDemo : MonoBehaviour
    {
        public RawImage selectPhoto = null; //头像
        public Button selectPhoto_btn = null; //选择头像按钮
        public RawImage uploadPhoto = null; //头像
        public Button uploadPhoto_btn = null; //上传头像按钮
        public RawImage takePhoto = null; //头像
        public Button takePhoto_btn = null; //上传头像按钮
        public string uploadUrl = "http://localhost:9090/upload"; //上传地址

        private string currentPath = null; //当前文件路径
        private Texture2D currentTexture2D = null; //当前图片Texture
        private string currentFileName = null; //当前文件名称
        private byte[] currentFileData = null; //当前文件数据

        private void Awake()
        {
            selectPhoto_btn.GetComponent<Button>().onClick.AddListener(() => SelectPhoto());
            uploadPhoto_btn.GetComponent<Button>().onClick.AddListener(() => UploadFile());
            takePhoto_btn.GetComponent<Button>().onClick.AddListener(() => TakePhoto());
        }

        /// <summary>
        /// 从手机相册选择头像
        /// </summary>
        public void SelectPhoto()
        {
            PhotoModule.SelectPhoto((path) =>
            {
                Debug.LogError("Image path: " + path);
                if (path != null)
                {
                    // 此Action为选取图片后的回调，返回一个Texture2D 
                    Texture2D texture = NativeGallery.LoadImageAtPath(path, PhotoModule.LoadImageMaxSize);
                    if (texture == null)
                    {
                        Debug.LogError("Couldn't load texture from " + path);
                        return;
                    }

                    Vector2 selectionVector = PhotoModule.GetCropperVector2(texture);
                    Debug.LogError("Texture Width:" + texture.width + ",Texture Height:" + texture.height);

                    var setting = new ImageCropper.Settings();
                    setting.selectionMinSize = selectionVector;
                    setting.selectionMaxSize = selectionVector;
                    setting.pixelPerfectSelection = true;

                    var fileName = FileModule.GetFileNameFromPath(path);

                    PhotoModule.CropperPhoto(texture, setting, (result, originalImage, croppedImage) =>
                    {
                        if (result)
                        {
                            // 裁剪成功，处理裁剪后的图像
                            Debug.Log("裁剪成功");
                            var targetRawImage = selectPhoto;
                            var texture2d = FileModule.DeepCopyTexture(croppedImage);
                            RefreshPhoto(fileName, "", texture2d, targetRawImage);
                        }
                        else
                        {
                            // 裁剪取消
                            Debug.Log("裁剪取消");
                        }
                    });
                }
            });
        }

        public void UploadFile()
        {
            if (currentTexture2D != null)
            {
                string fileName = currentFileName;
                if (currentFileData != null)
                {
                    StartCoroutine(FileModule.UploadFile(uploadUrl, currentFileData, fileName,
                        (newFileName, fileUrl) => { DownloadFile(fileUrl); }));
                }
            }
            else
            {
                Debug.LogError("请拍照或者至少选择一张图片...");
            }
        }

        public void DownloadFile(string fileUrl)
        {
            var savePath = FileModule.GetDownloadFolderPath();
            // 如果文件存在,直接加载
            // if (FileModule.CheckDownloadFileExist(fileUrl, savePath))
            // {
            //     FileModule.DownloadFileByUrlForNative(fileUrl, savePath, (fileName, fileData) =>
            //     {
            //         if (fileData != null)
            //         {
            //             savePath = Path.Combine(savePath, fileName);
            //             Texture2D texture = FileModule.FileData2Texture(fileData);
            //             var targetRawImage = uploadPhoto;
            //             RefreshPhoto(fileName, savePath, texture, targetRawImage);
            //         }
            //     });
            // }
            // else
            // {
            //     StartCoroutine(FileModule.DownloadFileByUrl(fileUrl, savePath, (fileName, fileData) =>
            //     {
            //         if (fileData != null)
            //         {
            //             savePath = Path.Combine(savePath, fileName);
            //             Texture2D texture = FileModule.FileData2Texture(fileData);
            //             var targetRawImage = uploadPhoto;
            //             RefreshPhoto(fileName, savePath, texture, targetRawImage);
            //         }
            //     }));
            // }

            StartCoroutine(FileModule.DownloadFileByUrl(fileUrl, savePath, (fileName, fileData) =>
            {
                if (fileData != null)
                {
                    savePath = Path.Combine(savePath, fileName);
                    Texture2D texture = FileModule.FileData2Texture(fileData);
                    var targetRawImage = uploadPhoto;
                    RefreshPhoto(fileName, savePath, texture, targetRawImage);
                }
            }));
        }

        public void TakePhoto()
        {
            PhotoModule.TakePhoto((path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Debug.Log("照片已保存至：" + path);
                    Texture2D texture = FileModule.LoadImageByPath(path);
                    var fileName = FileModule.GetFileNameFromPath(path);
                    // var targetRawImage = takePhoto;
                    // RefreshPhoto(fileName, path, texture, targetRawImage);

                    Vector2 selectionVector = PhotoModule.GetCropperVector2(texture);
                    Debug.LogError("Texture Width:" + texture.width + ",Texture Height:" + texture.height);

                    var setting = new ImageCropper.Settings();
                    setting.selectionMinSize = selectionVector;
                    setting.selectionMaxSize = selectionVector;
                    setting.pixelPerfectSelection = true;

                    PhotoModule.CropperPhoto(texture, setting, (result, originalImage, croppedImage) =>
                    {
                        if (result)
                        {
                            // 裁剪成功，处理裁剪后的图像
                            Debug.Log("裁剪成功");
                            var targetRawImage = takePhoto;
                            var texture2d = FileModule.DeepCopyTexture(croppedImage);
                            RefreshPhoto(fileName, "", texture2d, targetRawImage);
                        }
                        else
                        {
                            // 裁剪取消
                            Debug.Log("裁剪取消");
                        }
                    });
                }
                else
                {
                    Debug.LogError("未能拍摄照片");
                }
            });
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        private void RefreshPhoto(string fileName, string path = null, Texture2D texture = null,
            RawImage targetRawImage = null)
        {
            Debug.LogError("刷新显示");
            currentFileName = fileName;
            if (path != null)
            {
                currentPath = path;
            }

            if (texture != null)
            {
                currentTexture2D = texture;
                if (targetRawImage != null)
                {
                    targetRawImage.texture = texture;
                    currentFileData = FileModule.GetFileData(fileName, targetRawImage);
                }
            }
        }
    }
}