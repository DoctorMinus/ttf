using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;


namespace FontExport
{
    public static class WordGraph
    {
        private const int GGO_NATIVE = 2;
        private static StringBuilder outline = new StringBuilder();
        /// <summary>
        /// 获取轮廓多边形列表
        /// </summary>
        /// <param name="hdc">场景</param>
        /// <param name="uChar">字符</param>
        /// <returns></returns>
        private static String GetOutline(IntPtr hdc, uint uChar)
        {
            outline.Clear();

            // 转置矩阵
            MAT2 mat2 = new MAT2();
            mat2.eM11.value = 1;
            mat2.eM22.value = 1;

            GLYPHMETRICS lpGlyph = new GLYPHMETRICS();

            //获取缓存区大小
            int bufferSize = GdiNativeMethods.GetGlyphOutline(hdc, uChar, GGO_NATIVE, out lpGlyph, 0, IntPtr.Zero, ref mat2);

            if (bufferSize > 0)
            {
                //获取成功后，分配托管内存
                IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    int ret = GdiNativeMethods.GetGlyphOutline(hdc, uChar, GGO_NATIVE, out lpGlyph, (uint)bufferSize, buffer, ref mat2);
                    if (ret > 0)
                    {
                        //构建轮廓
                     
                        // width, height
                        outline.AppendFormat("{0},{1},$$POLY_SIZE$$,", lpGlyph.gmBlackBoxX, lpGlyph.gmBlackBoxY);
                        //从缓存区构造字型轮廓
                        GetPolygons(buffer, bufferSize);

                        return outline.ToString();
                    }
                    else
                    {
                        throw new Exception("获取字型数据失败！");
                    }
                }
                finally
                {
                    //释放托管内存
                    Marshal.FreeHGlobal(buffer);
                }
            }
            else
            {
                throw new Exception("未能获取缓存区！");
            }
        }

        /// <summary>
        /// 从缓存区构造多边形填充到字型轮廓(一个字型轮廓包含多个多边形，每个多边形由若干个直线或曲线组成)
        /// </summary>
        /// <param name="outline">轮廓</param>
        /// <param name="buffer">缓存区指针</param>
        /// <param name="bufferSize">缓存区大小</param>
        /// <returns></returns>
        private static void GetPolygons(IntPtr buffer, int bufferSize)
        {
            //多边形头大小
            int polygonHeaderSize = Marshal.SizeOf(typeof(TTPOLYGONHEADER));
            //线大小
            int lineSize = Marshal.SizeOf(typeof(TTPOLYCURVEHEAD));
            //点大小
            int pointFxSize = Marshal.SizeOf(typeof(POINTFX));

            //缓存区首地址值
            int ptr = buffer.ToInt32();
            //偏移量
            int offset = 0;

            //轮廓的多边形列表
            int poly_size = 0;
            while (offset < bufferSize)
            {
                //多边形头信息
                TTPOLYGONHEADER header = (TTPOLYGONHEADER)Marshal.PtrToStructure(new IntPtr(ptr + offset), typeof(TTPOLYGONHEADER));
                //构建多边形
                //DPolygon polygon = new DPolygon(header.dwType);
                //polygons.AppendFormat("polygon type: {0}\r\n", header.dwType );
                StringBuilder  poly = new StringBuilder();
                //起始点
                poly.AppendFormat("{0},{1},$$LINE_SIZE$$,", header.pfxStart.x, header.pfxStart.y);
                int line_size = 0;
                //获取尾索引
                int endCurvesIndex = offset + header.cb;
                //向后偏移一个项
                offset += polygonHeaderSize;

                while (offset < endCurvesIndex)
                {
                    //线段信息
                    TTPOLYCURVEHEAD lineHeader = (TTPOLYCURVEHEAD)Marshal.PtrToStructure(new IntPtr(ptr + offset), typeof(TTPOLYCURVEHEAD));
                    //偏移到点序列首地址
                    offset += lineSize;

                    //构建线段
                    //DLine line = new DLine(lineHeader.wType);
                    //polygons.AppendFormat("line type: ({0},{1})\r\npoints:\r\n", lineHeader.wType, lineHeader.wType == 1 ? "line" : "bezier");
                    //polygons.AppendFormat("{0},", lineHeader.wType);
                    StringBuilder line = new StringBuilder();
                    int points_size = 0;
                    
                    line.AppendFormat("{0},$$POINT_SIZE$$,", lineHeader.wType);
                    //读取点序列，加入线段中
                    for (int i = 0; i < lineHeader.cpfx; i++)
                    {
                        POINTFX point = (POINTFX)Marshal.PtrToStructure(new IntPtr(ptr + offset), typeof(POINTFX));
                        //将点加入线段
                        //line.Points.Add(point);
                        line.AppendFormat("{0},{1},", point.x, point.y);
                        points_size++;
                        //偏移
                        offset += pointFxSize;
                    }
                    line.Replace("$$POINT_SIZE$$", points_size.ToString());
                    poly.Append(line);
                    line_size++;
                    //将线加入多边形
                    //polygon.Lines.Add(line);
                }
                poly.Replace("$$LINE_SIZE$$", line_size.ToString());
                outline.Append(poly);
                poly_size++;
                //将多边形加入轮廓
                //polygons.Add(polygon);
            }
            outline.Length--;
            outline.Replace("$$POLY_SIZE$$", poly_size.ToString());
        }

        /// <summary>
        /// 获取指定字符在指定字体下的轮廓
        /// </summary>
        /// <param name="uChar">字符</param>
        /// <param name="font">字体</param>
        /// <returns></returns>
        public static String GetOutline(uint uChar, Font font)
        {
            //画板
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr hdc = g.GetHdc();

            //将字体选入场景
            IntPtr fontPtr = font.ToHfont();
            GdiNativeMethods.SelectObject(hdc, fontPtr);
            try
            {
                return GetOutline(hdc, uChar);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return "";
            }
        
        }
    }
}
