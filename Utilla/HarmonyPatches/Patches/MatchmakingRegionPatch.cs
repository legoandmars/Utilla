// Putting this here so lemming can put it into the base game wihtout fear of
// leagal reprucussion

// This work (this file) is licensed under the CC0 1.0 Universal licesnse, as
// detailed below
//
// Creative Commons Legal Code
//
// CC0 1.0 Universal
//
//     CREATIVE COMMONS CORPORATION IS NOT A LAW FIRM AND DOES NOT PROVIDE
//     LEGAL SERVICES. DISTRIBUTION OF THIS DOCUMENT DOES NOT CREATE AN
//     ATTORNEY-CLIENT RELATIONSHIP. CREATIVE COMMONS PROVIDES THIS
//     INFORMATION ON AN "AS-IS" BASIS. CREATIVE COMMONS MAKES NO WARRANTIES
//     REGARDING THE USE OF THIS DOCUMENT OR THE INFORMATION OR WORKS
//     PROVIDED HEREUNDER, AND DISCLAIMS LIABILITY FOR DAMAGES RESULTING FROM
//     THE USE OF THIS DOCUMENT OR THE INFORMATION OR WORKS PROVIDED
//     HEREUNDER.
//
// Statement of Purpose
//
// The laws of most jurisdictions throughout the world automatically confer
// exclusive Copyright and Related Rights (defined below) upon the creator
// and subsequent owner(s) (each and all, an "owner") of an original work of
// authorship and/or a database (each, a "Work").
//
// Certain owners wish to permanently relinquish those rights to a Work for
// the purpose of contributing to a commons of creative, cultural and
// scientific works ("Commons") that the public can reliably and without fear
// of later claims of infringement build upon, modify, incorporate in other
// works, reuse and redistribute as freely as possible in any form whatsoever
// and for any purposes, including without limitation commercial purposes.
// These owners may contribute to the Commons to promote the ideal of a free
// culture and the further production of creative, cultural and scientific
// works, or to gain reputation or greater distribution for their Work in
// part through the use and efforts of others.
//
// For these and/or other purposes and motivations, and without any
// expectation of additional consideration or compensation, the person
// associating CC0 with a Work (the "Affirmer"), to the extent that he or she
// is an owner of Copyright and Related Rights in the Work, voluntarily
// elects to apply CC0 to the Work and publicly distribute the Work under its
// terms, with knowledge of his or her Copyright and Related Rights in the
// Work and the meaning and intended legal effect of CC0 on those rights.
//
// 1. Copyright and Related Rights. A Work made available under CC0 may be
// protected by copyright and related or neighboring rights ("Copyright and
// Related Rights"). Copyright and Related Rights include, but are not
// limited to, the following:
//
//   i. the right to reproduce, adapt, distribute, perform, display,
//      communicate, and translate a Work;
//  ii. moral rights retained by the original author(s) and/or performer(s);
// iii. publicity and privacy rights pertaining to a person's image or
//      likeness depicted in a Work;
//  iv. rights protecting against unfair competition in regards to a Work,
//      subject to the limitations in paragraph 4(a), below;
//   v. rights protecting the extraction, dissemination, use and reuse of data
//      in a Work;
//  vi. database rights (such as those arising under Directive 96/9/EC of the
//      European Parliament and of the Council of 11 March 1996 on the legal
//      protection of databases, and under any national implementation
//      thereof, including any amended or successor version of such
//      directive); and
// vii. other similar, equivalent or corresponding rights throughout the
//      world based on applicable law or treaty, and any national
//      implementations thereof.
//
// 2. Waiver. To the greatest extent permitted by, but not in contravention
// of, applicable law, Affirmer hereby overtly, fully, permanently,
// irrevocably and unconditionally waives, abandons, and surrenders all of
// Affirmer's Copyright and Related Rights and associated claims and causes
// of action, whether now known or unknown (including existing as well as
// future claims and causes of action), in the Work (i) in all territories
// worldwide, (ii) for the maximum duration provided by applicable law or
// treaty (including future time extensions), (iii) in any current or future
// medium and for any number of copies, and (iv) for any purpose whatsoever,
// including without limitation commercial, advertising or promotional
// purposes (the "Waiver"). Affirmer makes the Waiver for the benefit of each
// member of the public at large and to the detriment of Affirmer's heirs and
// successors, fully intending that such Waiver shall not be subject to
// revocation, rescission, cancellation, termination, or any other legal or
// equitable action to disrupt the quiet enjoyment of the Work by the public
// as contemplated by Affirmer's express Statement of Purpose.
//
// 3. Public License Fallback. Should any part of the Waiver for any reason
// be judged legally invalid or ineffective under applicable law, then the
// Waiver shall be preserved to the maximum extent permitted taking into
// account Affirmer's express Statement of Purpose. In addition, to the
// extent the Waiver is so judged Affirmer hereby grants to each affected
// person a royalty-free, non transferable, non sublicensable, non exclusive,
// irrevocable and unconditional license to exercise Affirmer's Copyright and
// Related Rights in the Work (i) in all territories worldwide, (ii) for the
// maximum duration provided by applicable law or treaty (including future
// time extensions), (iii) in any current or future medium and for any number
// of copies, and (iv) for any purpose whatsoever, including without
// limitation commercial, advertising or promotional purposes (the
// "License"). The License shall be deemed effective as of the date CC0 was
// applied by Affirmer to the Work. Should any part of the License for any
// reason be judged legally invalid or ineffective under applicable law, such
// partial invalidity or ineffectiveness shall not invalidate the remainder
// of the License, and in such case Affirmer hereby affirms that he or she
// will not (i) exercise any of his or her remaining Copyright and Related
// Rights in the Work or (ii) assert any associated claims and causes of
// action with respect to the Work, in either case contrary to Affirmer's
// express Statement of Purpose.
//
// 4. Limitations and Disclaimers.
//
//  a. No trademark or patent rights held by Affirmer are waived, abandoned,
//     surrendered, licensed or otherwise affected by this document.
//  b. Affirmer offers the Work as-is and makes no representations or
//     warranties of any kind concerning the Work, express, implied,
//     statutory or otherwise, including without limitation warranties of
//     title, merchantability, fitness for a particular purpose, non
//     infringement, or the absence of latent or other defects, accuracy, or
//     the present or absence of errors, whether or not discoverable, all to
//     the greatest extent permissible under applicable law.
//  c. Affirmer disclaims responsibility for clearing rights of other persons
//     that may apply to the Work or any use thereof, including without
//     limitation any person's Copyright and Related Rights in the Work.
//     Further, Affirmer disclaims responsibility for obtaining any necessary
//     consents, permissions or other rights required for any use of the
//     Work.
//  d. Affirmer understands and acknowledges that Creative Commons is not a
//     party to this document and has no duty or obligation with respect to
//     this CC0 or use of the Work.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(PhotonNetworkController))]
	[HarmonyPatch("ProcessJoiningPublicRoomState", MethodType.Normal)]
	internal class MatchmakingRegionPatch
	{
		static bool firstRegion = true;
		static List<int> usedRegions = new List<int>();

		private static bool Prefix(PhotonNetworkController __instance, PhotonNetworkController.ConnectionEvent connectionEvent, ref bool ___joiningWithFriend, ref bool ___pastFirstConnection, ref int[] ___playersInRegion)
		{
			switch (connectionEvent)
			{
				// Attempting to join a room, try and join a random room ignoring used regions
				case PhotonNetworkController.ConnectionEvent.OnDisconnected:
					// These code paths do different things, don't alter them
					if (___joiningWithFriend || !___pastFirstConnection) return true;

					Debug.Log("attempt to join master and join a random room");

					// Ignore used reigons
					int[] playersInRegion = (int[])___playersInRegion.Clone();
					foreach (int i in usedRegions)
					{
						playersInRegion[i] = 0;
					}

					// Lemming's code, choose a random region weighted with the players in each region
					float value = UnityEngine.Random.value;
					int num = 0;
					for (int i = 0; i < playersInRegion.Length; i++)
					{
						num += playersInRegion[i];
					}
					float num2 = 0f;
					int num3 = -1;
					while (num2 < value && num3 < playersInRegion.Length - 1)
					{
						num3++;
						num2 += (float)playersInRegion[num3] / (float)num;
					}
					PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = __instance.serverRegions[num3];
					Debug.Log("joining past the first room, so we weighted randomly picked " + __instance.serverRegions[num3]);
					// End of lemming's code

					// For some reason the first time this is called it just runs again, so don't ignore the region we just chose
					if (!firstRegion)
					{
						usedRegions.Add(num3);
					}
					firstRegion = false;

					PhotonNetwork.ConnectUsingSettings();

					return false;

				// Failed to join a room in the region, try the next one 
				case PhotonNetworkController.ConnectionEvent.OnJoinRandomFailed:
					if (usedRegions.Count < __instance.serverRegions.Length)
					{
						// Not enough server regions used, so we will try again
						PhotonNetwork.Disconnect();
						return false;
					}
					else
					{
						// Tried all of the regions, make a new room
						return true;
					}

				// Successsfully joined room, so reset everything
				case PhotonNetworkController.ConnectionEvent.OnJoinedRoom:
					usedRegions = new List<int>();
					firstRegion = true;
					return true;

				// Not a case we care about, return
				default:
					return true;
			}
		}
	}
}
