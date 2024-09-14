using System;
using UnityEngine;

namespace Upload
{
    public static class PhotoModule
    {
        //加载图片最大尺寸
        public static int LoadImageMaxSize = 512;

        //最大截取图形尺寸
        public static int MaxSideSize = 512;

        //选择照片
        public static void SelectPhoto(Action<string> callBack = null)
        {
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => { callBack(path); }
            );
        }

        //裁切图片
        public static void CropperPhoto(Texture2D texture, ImageCropper.Settings settings = null,
            Action<bool, Texture, Texture2D> callBack = null)
        {
            if (texture == null)
            {
                Debug.LogError("please select a photo to cropper...");
                return;
            }

            if (!ImageCropper.Instance.IsOpen)
            {
                // 开始裁剪图像
                ImageCropper.Instance.Show(texture,
                    (bool result, Texture originalImage, Texture2D croppedImage) =>
                    {
                        if (callBack != null)
                        {
                            callBack(result, originalImage, croppedImage);
                        }
                    }, settings);
            }
            else
            {
                Debug.LogError("cropper is open...");
            }
        }

        //拍摄照片
        public static void TakePhoto(Action<string> callBack = null)
        {
            NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
            {
                if (callBack != null)
                {
                    callBack(path);
                }
            });
        }

        //获取图片短边
        public static float GetTextureShorterSize(Texture2D texture2D)
        {
            if (texture2D != null)
            {
                float width = texture2D.width;
                float height = texture2D.height;

                float shorterSize = height;
                if (width <= height)
                {
                    shorterSize = width;
                }

                if (shorterSize >= MaxSideSize)
                {
                    shorterSize = MaxSideSize;
                }

                return shorterSize;
            }

            return 0;
        }

        //获取截取的Vector2
        public static Vector2 GetCropperVector2(Texture2D texture2D)
        {
            if (texture2D != null)
            {
                float sideSize = GetTextureShorterSize(texture2D);
                return new Vector2(sideSize, sideSize);
            }

            return Vector2.zero;
        }
    }
}