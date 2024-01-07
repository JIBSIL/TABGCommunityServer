using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABGCommunityServer
{
    internal class WeaponConcurrencyHandler
    {
        public Dictionary<int, Weapon> WeaponDB = new Dictionary<int, Weapon>();
        public int CurrentID = 0;

        public WeaponConcurrencyHandler() { }

        public void SpawnWeapon(Weapon weapon)
        {
            //WeaponDB[weapon.Id] = weapon;
            WeaponDB[weapon.Type] = weapon;

            CurrentID++;
        }

        public void RemoveWeapon(Weapon weapon)
        {
            WeaponDB.Remove(weapon.Id);
        }
    }
}
