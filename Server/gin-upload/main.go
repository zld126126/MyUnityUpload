package main

import (
	"fmt"
	"io"
	"net/http"
	"os"
	"path/filepath"

	"github.com/gin-gonic/gin"
)

// 确保目录存在，如果不存在则创建它
func ensureDirExists(dir string) error {
	if _, err := os.Stat(dir); os.IsNotExist(err) {
		return os.MkdirAll(dir, os.ModePerm)
	}
	return nil
}

func main() {
	port := ":9090"

	r := gin.Default()

	// 确保上传目录存在
	if err := ensureDirExists("uploads"); err != nil {
		panic("Failed to create uploads directory: " + err.Error())
	}

	// 设置静态文件服务
	r.Static("/uploads", "./uploads")

	// 提供 HTML 上传表单页面
	r.GET("/", func(c *gin.Context) {
		c.HTML(http.StatusOK, "upload.html", nil)
	})

	// 处理文件上传
	r.POST("/uploadForHtml", func(c *gin.Context) {
		// 从请求中获取文件
		file, header, err := c.Request.FormFile("file")
		if err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Failed to get file"})
			return
		}

		fileName := header.Filename
		filePath := filepath.Join("uploads", fileName)
		// 创建目标文件，自动覆盖已存在的文件
		out, err := os.Create(filePath)
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create file: " + err.Error()})
			return
		}
		defer out.Close()

		// 将上传的文件内容复制到目标文件
		if _, err := io.Copy(out, file); err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to save file: " + err.Error()})
			return
		}

		// 构造文件的 URL
		fileURL := fmt.Sprintf("http://localhost"+port+"/uploads/%s", fileName)

		// 返回上传成功的信息和文件 URL
		c.HTML(http.StatusOK, "result.html", gin.H{
			"fileURL": fileURL,
		})
	})

	// 处理文件上传
	r.POST("/upload", func(c *gin.Context) {
		// 从请求中获取文件
		file, header, err := c.Request.FormFile("file")
		if err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Failed to get file"})
			return
		}

		fileName := header.Filename
		filePath := filepath.Join("uploads", fileName)
		// 创建目标文件，自动覆盖已存在的文件
		out, err := os.Create(filePath)
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create file: " + err.Error()})
			return
		}
		defer out.Close()

		// 将上传的文件内容复制到目标文件
		if _, err := io.Copy(out, file); err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to save file: " + err.Error()})
			return
		}

		// 构造文件的 URL
		fileURL := fmt.Sprintf("http://localhost"+port+"/uploads/%s", fileName)
		// 返回上传成功的信息和文件 URL
		c.JSON(http.StatusOK, gin.H{
			"fileURL": fileURL,
		})
	})

	r.GET("/test", func(c *gin.Context) {
		c.String(http.StatusOK, "test success")
	})

	// 渲染 HTML 模板
	r.LoadHTMLGlob("templates/*")

	// 启动服务器，监听端口
	r.Run(port)
}
