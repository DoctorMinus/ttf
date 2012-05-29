using System;
using System.Collections.Generic;
using System.Text;

namespace TrueTypeText.Outline.Data
{
    /// <summary>
    /// ������
    /// </summary>
    public class DLine
    {
        private int type;
        private IList<POINTFX> points;

        public DLine(int type)
        {
            this.type = type;
            this.points = new List<POINTFX>();
        }
        /// <summary>
        /// ��ȡ����
        /// </summary>
        public int Type
        {
            get { return type; }
        }
        /// <summary>
        /// ��ȡ������
        /// </summary>
        public IList<POINTFX> Points
        {
            get { return points; }
        }
    }
}