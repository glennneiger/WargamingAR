﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using WAR.Units;
using WAR.Game;
using WAR.Equipment;

namespace WAR.Game {
	public enum DamageType {
		emp,
		thermal,
		phaser,
		kinetic,
		special,
	}
	
	public struct Damage {
		[Range(1,4)]
		public int strength;
		[Range(1,4)]
		public int armorPen;
	}
	
	public class ShootingAttack : IWARAttack {
		public DamageProfile damage;	
		public DamageProfile armor;
		public int range;
		public int accuracy;
		public int attacks;
		public bool requireLOS;
		public int weaponSkill;
		public WeaponType weaponType;
		
		public DamageProfile getAttack() {
			return damage;
		}
		
		public DamageProfile getArmor() {
			return armor;
		}
		
		public ShootingAttack() {
			damage = new DamageProfile();
			armor = new DamageProfile();
		}
	}
	
	public class WARShootingAttack {
		// who is making this shooting attack
		private WARUnit shooter;
		public ShootingAttack attack;
		
		// make the initial modification with the weapon that was fired
		public WARShootingAttack(WARUnit shooter, IWARShootingModifier fired) {
			this.shooter = shooter;
			// apply the initial profile 
			this.attack = fired == null ? new ShootingAttack() : fired.modifyShootingAttack(new ShootingAttack());
		}
		
		public WARShootingAttack() {
			this.shooter = null;
			// create an empty shooting attack
			this.attack = new ShootingAttack();
		}
		
		public ShootingAttack computeAttack() {
			// the list of equipment that contributes to a shooting event
			var equipment = shooter.GetComponents<MonoBehaviour>()
				.Where(eq => (eq as IWARShootingModifier) != null)
				// ignoring weapons since only one is applied to begin with
				.Where(eq => (eq as WARWeapon) == null)
				// cast them to the interface
				.Select(item => item as IWARShootingModifier);
			
			// start off with the initial weap modifier
			var result = attack;
			
			// for each piece of equipment we care about
			foreach (var item in equipment) {
				// apply the modifier
				result = item.modifyShootingAttack(result);
			} 
			
			// return the final attack
			return result;
		}
		
		public ShootingAttack computeFinalAttack(WARUnit target) {
			// the list of equipment that contributes to a shooting event
			var equipment = target.GetComponents<MonoBehaviour>()
				.Where(eq => (eq as IWARShootingTargetModifier) != null)
				// cast them to the interface
				.Select(item => item as IWARShootingTargetModifier);
			
			// start off with the offensives profile
			var result = computeAttack();
			
			// for each piece of equipment we care about
			foreach (var item in equipment) {
				// apply the modifier
				result = item.modifyShootingAttackTarget(result);
			}
			
			// return the final attack
			return result;
		}
	}
}
