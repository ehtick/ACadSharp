﻿using ACadSharp.Attributes;
using ACadSharp.Extensions;
using CSMath;
using System;

namespace ACadSharp.Tables
{
	public partial class LineType
	{
		public class Segment
		{
			/// <summary>
			/// Dash, dot or space length.
			/// </summary>
			[DxfCodeValue(49)]
			public double Length { get; set; }

			/// <summary>
			/// Line type where this segment belongs
			/// </summary>
			public LineType LineType { get; internal set; }

			/// <summary>
			/// Offset.
			/// </summary>
			[DxfCodeValue(44, 45)]
			public XY Offset { get; set; }

			/// <summary>
			/// Rotation value in radians of embedded shape or text.
			/// </summary>
			[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
			public double Rotation { get; set; }

			/// <summary>
			/// Scale value.
			/// </summary>
			[DxfCodeValue(46)]
			public double Scale { get; set; } = 1.0d;

			/// <summary>
			/// Complex linetype element type.
			/// </summary>
			[DxfCodeValue(74)]
			public LinetypeShapeFlags Shapeflag { get; set; }

			/// <summary>
			/// Shape number.
			/// </summary>
			[DxfCodeValue(75)]
			public short ShapeNumber { get; set; }

			/// <summary>
			/// Pointer to STYLE object (one per element if code 74 > 0)
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Handle, 340)]
			public TextStyle Style
			{
				get { return this._style; }
				set
				{
					this._style = updateTable(value, this.LineType?.Document?.TextStyles);
				}
			}

			/// <summary>
			/// Text string.
			/// </summary>
			/// <remarks>
			/// Only present if <see cref="LinetypeShapeFlags.Text"/> is present
			/// </remarks>
			[DxfCodeValue(9)]
			public string Text
			{
				get { return this._text; }
				set
				{
					this._text = string.IsNullOrEmpty(value) ? string.Empty : value;
				}
			}

			private TextStyle _style = null;

			private string _text = string.Empty;

			public LineType.Segment Clone()
			{
				Segment clone = MemberwiseClone() as Segment;
				clone.LineType = null;
				clone._style = (TextStyle)(this.Style?.Clone());
				return clone;
			}

			internal void AssignDocument(CadDocument doc)
			{
				this._style = updateTable(this._style, doc.TextStyles);
			}

			internal void UnassignDocument()
			{
				this._style = this._style.CloneTyped();
			}
		}
	}
}