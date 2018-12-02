# EPIAS
EPİAŞ / EXIST

C# class for [EPİAŞ / EXIST][homepage_exist] [Metering Point Services][homepage_service]

| Function |  | Status |
| ------ | ------ | ------ |
| /cmp/list-changed-supplier-meters | List Meters whose supplier has changed | Testing |
| /cmp/list-deducted-meters | Deducted Meters Service | Not tested * |
| /cmp/list-meter-count | List Meter Counts | Not tested * |
| /cmp/list-meter-eic | Meter EIC Querying Service | Not tested * |
| /cmp/list-meter-eic-range | List Meter Eic List Service | Not tested * |
| /cmp/listall | Metering Point Listing Service | Done |

* Need an allowed account

[homepage_owner]: <https://www.progedia.com>
[homepage_product]: <https://www.progedia.com>
[github_owner]: <https://github.com/thefabal>
[homepage_exist]: <https://www.epias.com.tr>
[homepage_service]: <https://tys.epias.com.tr/ecms-consumption-metering-point/technical/en/>

# Usage
```C#
epias epias = new epias() {
  user_name = <exist_username>,
  user_pass = <exist_password>,
  insane_mode = <true|false>
};
```
