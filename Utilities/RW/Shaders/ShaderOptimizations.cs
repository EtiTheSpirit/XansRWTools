using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XansTools.Utilities.RW.Shaders {

	/// <summary>
	/// Provides optimizations to shaders by turning a lot of values into keywords for static branching.
	/// </summary>
	internal static class ShaderOptimizations {

		private static readonly PropertyBoundKeywordToggler _rimFix = new PropertyBoundKeywordToggler("RIMFIX_ON", "_rimFix", 0.5f);
		private static readonly PropertyBoundKeywordToggler _grime = new PropertyBoundKeywordToggler("GRIME_ON", "_Grime");
		private static readonly PropertyBoundKeywordToggler _swarmRoom = new PropertyBoundKeywordToggler("IS_SWARM_ROOM", "_SwarmRoom");

		internal static void Initialize() {
			Log.LogMessage("Implementing custom level color shader optimizations (static branching support)...");
			Shader.EnableKeyword("PROPERTY_STATIC_BRANCHES_AVAILABLE");

			On.RoomCamera.MoveCamera_Room_int += OnMovingCamera;
		}

		private static void OnMovingCamera(On.RoomCamera.orig_MoveCamera_Room_int originalMethod, RoomCamera @this, Room newRoom, int camPos) {
			originalMethod(@this, newRoom, camPos);
			_rimFix.Update();
			_grime.Update();
			_swarmRoom.Update();
		}

		private class KeywordToggler {

			public virtual bool Enabled {
				get => _desiredState;
				set {
					if (_desiredState != value) {
						_desiredState = value;
						if (value) {
							Shader.EnableKeyword(keyword);
						} else {
							Shader.DisableKeyword(keyword);
						}
					}
				}
			}
			private bool _desiredState = false;

			public readonly string keyword;

			public KeywordToggler(string keyword) {
				this.keyword = keyword;
			}

		}

		private class PropertyBoundKeywordToggler : KeywordToggler {

			public readonly string property;
			public readonly int propertyID;
			public readonly float threshold;
			public readonly bool whenGreater;

			public PropertyBoundKeywordToggler(string keyword, string property, float threshold = 0, bool whenGreater = true) : base(keyword) {
				this.property = property;
				this.threshold = threshold;
				this.whenGreater = whenGreater;
				propertyID = Shader.PropertyToID(property);
			}

			public void Update() {
				if (whenGreater) {
					Enabled = Shader.GetGlobalFloat(property) > threshold;
				} else {
					Enabled = Shader.GetGlobalFloat(property) < threshold;
				}
			}
		}
	}
}
