using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EPIAS {
    class Program {
        private static csettings settings = new csettings();

        static void Main( string[ ] args ) {
            settings.Get();
            if( settings.Check() == false ) {
                settings.Set();
            }

            epias epias = new epias() {
                user_name = settings.epias_username,
                user_pass = settings.epias_userpass,
                insane_mode = true
            };

            List<MeterDatas> response = epias.getMeterDataConfiguration( new DateTime(2018,11,1) );

            using( StreamWriter sw = new StreamWriter( "MeterDataConfiguration_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".txt" ) ) {
                foreach( MeterDatas item in response ) {
                    sw.Write( item.meterId + "," );
                    sw.Write( item.meterEic + "," );
                    sw.Write( item.meterCity + "," );
                    sw.Write( ( item.term == null ) ? ( "," ) : ( ( (DateTime)( item.term ) ).ToString( "yyyy-MM-dd HH:mm:ss" ) + "," ) );
                    sw.Write( ( item.dataVersion == null ) ? ( "," ) : ( ( (DateTime)( item.dataVersion ) ).ToString( "yyyy-MM-dd HH:mm:ss" ) + "," ) );
                    sw.Write( ( item.confVersion == null ) ? ( "," ) : ( ( (DateTime)( item.confVersion ) ).ToString( "yyyy-MM-dd HH:mm:ss" ) + "," ) );
                    sw.Write( item.meterConsumption + "," );
                    sw.Write( item.meterGeneration + "," );
                    sw.Write( item.meterLossyConsumption + "," );
                    sw.Write( item.meterLossyGeneration + "," );
                    sw.Write( item.readingType + "," );
                    sw.Write( item.supplierOrganization + "," );
                    sw.Write( item.meterReadingCompany + "," );
                    sw.Write( item.isConfWithdrawalSettlement + "," );
                    sw.Write( item.isConfSupplySettlement + "," );
                    sw.Write( item.isConfWithdDeducSettlement + "," );
                    sw.Write( item.isConfSupplyDeducSettlement + "," );
                    sw.Write( item.isRead + "," );
                    sw.WriteLine( "" );
                }
            }

            Console.ReadLine();
        }

        public class csettings {
            public string epias_username = string.Empty;
            public string epias_userpass = string.Empty;

            public void Get( ) {
                epias_username = Properties.Settings.Default.epias_username;
                epias_userpass = Properties.Settings.Default.epias_userpass;
            }

            public void Set( ) {
                Console.Write( "EPİAŞ / EXIST User Name : " );
                epias_username = Console.ReadLine();
                Console.Write( "EPİAŞ / EXIST Password : " );
                epias_userpass = Console.ReadLine();

                Save();
            }

            public void Show( ) {
                Console.WriteLine( "EPİAŞ / EXIST User Name : " + epias_username );
                Console.WriteLine( "EPİAŞ / EXIST Password : " + epias_userpass );
            }

            public void Save( ) {
                Properties.Settings.Default.epias_username = epias_username;
                Properties.Settings.Default.epias_userpass = epias_userpass;
                Properties.Settings.Default.Save();
            }

            public bool Check( ) {
                if( epias_username.Length == 0 || epias_userpass.Length == 0 ) {
                    return false;
                }

                return true;
            }
        }
    }
}
