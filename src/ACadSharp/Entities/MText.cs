﻿using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="MText"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMText"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MText"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMText)]
	[DxfSubClass(DxfSubclassMarker.MText)]
	public partial class MText : Entity, IText
	{
		/// <inheritdoc/>
		[DxfCodeValue(11, 21, 31)]
		public XYZ AlignmentPoint { get; set; } = XYZ.AxisX;

		/// <summary>
		/// Attachment point
		/// </summary>
		[DxfCodeValue(71)]
		public AttachmentPointType AttachmentPoint { get; set; } = AttachmentPointType.TopLeft;

		/// <summary>
		/// Background fill color
		/// </summary>
		/// <remarks>
		/// Color to use for background fill when group code 90 is 1.
		/// </remarks>
		[DxfCodeValue(63, 420, 430)]
		public Color BackgroundColor { get; set; }

		/// <summary>
		/// Background fill setting
		/// </summary>
		[DxfCodeValue(90)]
		public BackgroundFillFlags BackgroundFillFlags { get; set; } = BackgroundFillFlags.None;

		/// <summary>
		/// Determines how much border there is around the text.
		/// </summary>
		[DxfCodeValue(45)]
		public double BackgroundScale { get; set; } = 1.5;

		/// <summary>
		/// Transparency of background fill color
		/// </summary>
		[DxfCodeValue(441)]
		public Transparency BackgroundTransparency { get; set; }

		public TextColumn Column { get; set; } = new TextColumn();

		/// <summary>
		/// Drawing direction
		/// </summary>
		[DxfCodeValue(72)]
		public DrawingDirectionType DrawingDirection { get; set; } = DrawingDirectionType.LeftToRight;

		/// <inheritdoc/>
		[DxfCodeValue(40)]
		public double Height
		{
			get => this._height;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Height value cannot be negative.");
				else
					this._height = value;
			}
		}

		/// <summary>
		/// Horizontal width of the characters that make up the mtext entity.
		/// This value will always be equal to or less than the value of group code 41
		/// </summary>
		/// <remarks>
		/// read-only, ignored if supplied
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Ignored, 42)]
		public double HorizontalWidth { get; set; } = 0.9;

		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		public bool IsAnnotative { get; set; } = false;

		/// <summary>
		/// Mtext line spacing factor.
		/// </summary>
		/// <remarks>
		/// Percentage of default (3-on-5) line spacing to be applied.Valid values range from 0.25 to 4.00
		/// </remarks>
		[DxfCodeValue(44)]
		public double LineSpacing { get; set; } = 1.0;

		/// <summary>
		/// Mtext line spacing style.
		/// </summary>
		[DxfCodeValue(73)]
		public LineSpacingStyleType LineSpacingStyle { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMText;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.MTEXT;

		/// <summary>
		/// Reference rectangle height.
		/// </summary>
		[DxfCodeValue(46)]
		public double RectangleHeight { get; set; }

		/// <summary>
		/// Reference rectangle width.
		/// </summary>
		[DxfCodeValue(41)]
		public double RectangleWidth { get; set; }

		/// <inheritdoc/>
		/// <remarks>
		/// The rotation is only valid if the <see cref="Normal"/> is set to the Z axis.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.IsAngle | DxfReferenceType.Ignored, 50)]
		public double Rotation
		{
			get
			{
				return new XY(this.AlignmentPoint.X, this.AlignmentPoint.Y).GetAngle();
			}
		}

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Name | DxfReferenceType.Optional, 7)]
		public TextStyle Style
		{
			get { return this._style; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._style = updateTable(value, this.Document.TextStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MText;

		/// <inheritdoc/>
		[DxfCodeValue(1)]
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// Vertical height of the mtext entity
		/// </summary>
		/// <remarks>
		/// read-only, ignored if supplied
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Ignored, 43)]
		public double VerticalHeight { get; set; } = 0.2;

		private double _height = 0.18;

		private TextStyle _style = TextStyle.Default;

		/// <inheritdoc/>
		public MText() : base() { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			XYZ newInsert = transform.ApplyTransform(this.InsertPoint);
			XYZ newNormal = this.transformNormal(transform, this.Normal);

			var transformation = this.getWorldMatrix(transform, Normal, newNormal, out Matrix3 transOW, out Matrix3 transWO);

			transWO = transWO.Transpose();

			List<XY> uv = applyRotation(
				new[]
				{
					XY.AxisX, XY.AxisY
				},
				this.Rotation);

			XYZ v;
			v = transOW * new XYZ(uv[0].X, uv[0].Y, 0.0);
			v = transformation * v;
			v = transWO * v;
			XY newUvector = new XY(v.X, v.Y);

			// the MText entity does not support non-uniform scaling
			double scale = newUvector.GetLength();

			v = transOW * new XYZ(uv[1].X, uv[1].Y, 0.0);
			v = transformation * v;
			v = transWO * v;
			XY newVvector = new XY(v.X, v.Y);

			double newRotation = newUvector.GetAngle();

			if (XY.Cross(newUvector, newVvector) < 0.0)
			{
				if (newUvector.Dot(uv[0]) < 0.0)
				{
					newRotation += 180;

					switch (this.AttachmentPoint)
					{
						case AttachmentPointType.TopLeft:
							this.AttachmentPoint = AttachmentPointType.TopRight;
							break;
						case AttachmentPointType.TopRight:
							this.AttachmentPoint = AttachmentPointType.TopLeft;
							break;
						case AttachmentPointType.MiddleLeft:
							this.AttachmentPoint = AttachmentPointType.MiddleRight;
							break;
						case AttachmentPointType.MiddleRight:
							this.AttachmentPoint = AttachmentPointType.MiddleLeft;
							break;
						case AttachmentPointType.BottomLeft:
							this.AttachmentPoint = AttachmentPointType.BottomRight;
							break;
						case AttachmentPointType.BottomRight:
							this.AttachmentPoint = AttachmentPointType.BottomLeft;
							break;
					}
				}
				else
				{
					switch (this.AttachmentPoint)
					{
						case AttachmentPointType.TopLeft:
							this.AttachmentPoint = AttachmentPointType.BottomLeft;
							break;
						case AttachmentPointType.TopCenter:
							this.AttachmentPoint = AttachmentPointType.BottomCenter;
							break;
						case AttachmentPointType.TopRight:
							this.AttachmentPoint = AttachmentPointType.BottomRight;
							break;
						case AttachmentPointType.BottomLeft:
							this.AttachmentPoint = AttachmentPointType.TopLeft;
							break;
						case AttachmentPointType.BottomCenter:
							this.AttachmentPoint = AttachmentPointType.TopCenter;
							break;
						case AttachmentPointType.BottomRight:
							this.AttachmentPoint = AttachmentPointType.TopRight;
							break;
					}
				}
			}

			double newHeight = this.Height * scale;
			newHeight = MathHelper.IsZero(newHeight) ? MathHelper.Epsilon : newHeight;

			this.InsertPoint = newInsert;
			this.Normal = newNormal;
			this.Height = newHeight;
			this.RectangleWidth *= scale;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			MText clone = (MText)base.Clone();

			clone.Style = (TextStyle)(this.Style?.Clone());
			clone.Column = this.Column?.Clone();

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.InsertPoint);
		}

		/// <summary>
		/// Get the text value separated in lines.
		/// </summary>
		/// <returns></returns>
		public string[] GetTextLines()
		{
			return this.Value.Split(
				new string[] { "\r\n", "\r", "\n", "\\P" },
				StringSplitOptions.None
			);
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = updateTable(this.Style, doc.TextStyles);

			doc.DimensionStyles.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.DimensionStyles.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Style = (TextStyle)this.Style.Clone();
		}

		protected override void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			base.tableOnRemove(sender, e);

			if (e.Item.Equals(this.Style))
			{
				this.Style = this.Document.TextStyles[TextStyle.DefaultName];
			}
		}
	}
}