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
        /// ��ȡ����������б�
        /// </summary>
        /// <param name="hdc">����</param>
        /// <param name="uChar">�ַ�</param>
        /// <returns></returns>
        private static String GetOutline(IntPtr hdc, uint uChar)
        {
            outline.Clear();

            // ת�þ���
            MAT2 mat2 = new MAT2();
            mat2.eM11.value = 1;
            mat2.eM22.value = 1;

            GLYPHMETRICS lpGlyph = new GLYPHMETRICS();

            //��ȡ��������С
            int bufferSize = GdiNativeMethods.GetGlyphOutline(hdc, uChar, GGO_NATIVE, out lpGlyph, 0, IntPtr.Zero, ref mat2);

            if (bufferSize > 0)
            {
                //��ȡ�ɹ��󣬷����й��ڴ�
                IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    int ret = GdiNativeMethods.GetGlyphOutline(hdc, uChar, GGO_NATIVE, out lpGlyph, (uint)bufferSize, buffer, ref mat2);
                    if (ret > 0)
                    {
                        //��������
                     
                        // width, height
                        outline.AppendFormat("{0},{1},$$POLY_SIZE$$,", lpGlyph.gmBlackBoxX, lpGlyph.gmBlackBoxY);
                        //�ӻ�����������������
                        GetPolygons(buffer, bufferSize);

                        return outline.ToString();
                    }
                    else
                    {
                        throw new Exception("��ȡ��������ʧ�ܣ�");
                    }
                }
                finally
                {
                    //�ͷ��й��ڴ�
                    Marshal.FreeHGlobal(buffer);
                }
            }
            else
            {
                throw new Exception("δ�ܻ�ȡ��������");
            }
        }

        /// <summary>
        /// �ӻ���������������䵽��������(һ���������������������Σ�ÿ������������ɸ�ֱ�߻��������)
        /// </summary>
        /// <param name="outline">����</param>
        /// <param name="buffer">������ָ��</param>
        /// <param name="bufferSize">��������С</param>
        /// <returns></returns>
        private static void GetPolygons(IntPtr buffer, int bufferSize)
        {
            //�����ͷ��С
            int polygonHeaderSize = Marshal.SizeOf(typeof(TTPOLYGONHEADER));
            //�ߴ�С
            int lineSize = Marshal.SizeOf(typeof(TTPOLYCURVEHEAD));
            //���С
            int pointFxSize = Marshal.SizeOf(typeof(POINTFX));

            //�������׵�ֵַ
            int ptr = buffer.ToInt32();
            //ƫ����
            int offset = 0;

            //�����Ķ�����б�
            int poly_size = 0;
            while (offset < bufferSize)
            {
                //�����ͷ��Ϣ
                TTPOLYGONHEADER header = (TTPOLYGONHEADER)Marshal.PtrToStructure(new IntPtr(ptr + offset), typeof(TTPOLYGONHEADER));
                //���������
                //DPolygon polygon = new DPolygon(header.dwType);
                //polygons.AppendFormat("polygon type: {0}\r\n", header.dwType );
                StringBuilder  poly = new StringBuilder();
                //��ʼ��
                poly.AppendFormat("{0},{1},$$LINE_SIZE$$,", header.pfxStart.x, header.pfxStart.y);
                int line_size = 0;
                //��ȡβ����
                int endCurvesIndex = offset + header.cb;
                //���ƫ��һ����
                offset += polygonHeaderSize;

                while (offset < endCurvesIndex)
                {
                    //�߶���Ϣ
                    TTPOLYCURVEHEAD lineHeader = (TTPOLYCURVEHEAD)Marshal.PtrToStructure(new IntPtr(ptr + offset), typeof(TTPOLYCURVEHEAD));
                    //ƫ�Ƶ��������׵�ַ
                    offset += lineSize;

                    //�����߶�
                    //DLine line = new DLine(lineHeader.wType);
                    //polygons.AppendFormat("line type: ({0},{1})\r\npoints:\r\n", lineHeader.wType, lineHeader.wType == 1 ? "line" : "bezier");
                    //polygons.AppendFormat("{0},", lineHeader.wType);
                    StringBuilder line = new StringBuilder();
                    int points_size = 0;
                    
                    line.AppendFormat("{0},$$POINT_SIZE$$,", lineHeader.wType);
                    //��ȡ�����У������߶���
                    for (int i = 0; i < lineHeader.cpfx; i++)
                    {
                        POINTFX point = (POINTFX)Marshal.PtrToStructure(new IntPtr(ptr + offset), typeof(POINTFX));
                        //��������߶�
                        //line.Points.Add(point);
                        line.AppendFormat("{0},{1},", point.x, point.y);
                        points_size++;
                        //ƫ��
                        offset += pointFxSize;
                    }
                    line.Replace("$$POINT_SIZE$$", points_size.ToString());
                    poly.Append(line);
                    line_size++;
                    //���߼�������
                    //polygon.Lines.Add(line);
                }
                poly.Replace("$$LINE_SIZE$$", line_size.ToString());
                outline.Append(poly);
                poly_size++;
                //������μ�������
                //polygons.Add(polygon);
            }
            outline.Length--;
            outline.Replace("$$POLY_SIZE$$", poly_size.ToString());
        }

        /// <summary>
        /// ��ȡָ���ַ���ָ�������µ�����
        /// </summary>
        /// <param name="uChar">�ַ�</param>
        /// <param name="font">����</param>
        /// <returns></returns>
        public static String GetOutline(uint uChar, Font font)
        {
            //����
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr hdc = g.GetHdc();

            //������ѡ�볡��
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
