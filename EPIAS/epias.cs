using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.IO;
using System.Net;
using System.Net.Cache;
using System.Web.Script.Serialization;

namespace EPIAS {
    class epias {
        public int count_perrun { get; set; } = 1000;
        public bool insane_mode { get; set; } = false;
        public string user_name { get; set; } = string.Empty;
        public string user_pass { get; set; } = string.Empty;

        private readonly string tgt_url = "https://cas.epias.com.tr/cas/v1/tickets?format=text";
        private readonly string tys_url = "https://tys.epias.com.tr";
        private readonly string mdc_url = "ecms-consumption-metering-point/rest/metering/data/total/list-meter-data-configuration?format=json";
        private readonly string csm_url = "ecms-consumption-metering-point/rest/cmp/list-changed-supplier-meters?format=json";

        private string tgt = string.Empty;
        private string st = string.Empty;
        private static Stopwatch swTGT = new Stopwatch();

        /**
         * Service to control meter is read or not and Listing past meters 
         **/
        public List<MeterDatas> getMeterDataConfiguration( DateTime term, bool version = false ) {
            List<MeterDatas> meterDatas = new List<MeterDatas>();
            responseMeteringDataConfiguration partial;

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    partial = getMeterDataConfiguration( term, 0, 1, version );

                    break;
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw new Exception( "error on getting meter data configuration" );
                    }
                }
            }

            /**
             * get all records.
             **/
            int num_of_record = partial.body.queryInformation.count;
            for(int i = 0; i < num_of_record; i += count_perrun ) {
                while(true) {
                    try {
                        meterDatas = meterDatas.Concat( getMeterDataConfiguration( term, i, Math.Min( i + count_perrun - 1, num_of_record ), version ).body.meterDatas ).ToList();

                        break;
                    } catch(Exception ex) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw new Exception( "error on getting meter data configuration" );
                        }
                    }
                }
            }

            return meterDatas;
        }

        private responseMeteringDataConfiguration getMeterDataConfiguration( DateTime term, int range_begin, int range_end, bool pastVersion ) {
            if( getST() == false ) {
                return new responseMeteringDataConfiguration();
            }

            string request = (new requestMeterDataConfiguration() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new mdcBody() {
                    term = term,
                    pastVersion = pastVersion,
                    meteringReadingType = "null",
                    range = new Range() {
                        begin = range_begin,
                        end = range_end
                    }
                }
            }).ToString();

            string response = postData( request, tys_url + "/" + mdc_url );
            if( response.Length != 0 ) {
                if( response.IndexOf( "SECURITYERROR" ) != -1 ) {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                } else {
                    return new JavaScriptSerializer().Deserialize<responseMeteringDataConfiguration>( response );
                }
            } else {
                return new responseMeteringDataConfiguration();
            }
        }

        /**
         * List Meters whose supplier has changed
         **/
        public List<ChangedSupplierMeter> getChangedSupplierMeters( DateTime term, string listType ) {
            List<ChangedSupplierMeter> changedSupplierMeterListResponse = new List<ChangedSupplierMeter>();
            responseChangedSupplierMeters partial;

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    partial = getChangedSupplierMeters( term, 0, 1, listType );

                    break;
                } catch( EXISTException ex) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw new Exception( "error on getting meter data configuration" );
                    }
                }
            }

            /**
             * get all records.
             **/
            int num_of_record = partial.body.queryInformation.count;
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        changedSupplierMeterListResponse = changedSupplierMeterListResponse.Concat( getChangedSupplierMeters( term, i, Math.Min( i + count_perrun - 1, num_of_record ), listType ).body.changedSupplierMeterListResponse ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw new Exception( "error on getting meter data configuration" );
                        }
                    }
                }
            }

            return changedSupplierMeterListResponse;
        }

        private responseChangedSupplierMeters getChangedSupplierMeters( DateTime term, int range_begin, int range_end, string listType ) {
            string request = ( new requestChangedSupplierMeters() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new csmBody() {
                    term = term,
                    listType = listType,
                    range = new Range() {
                        begin = range_begin,
                        end = range_end
                    }
                }
            } ).ToString();

            string response = postData( request, tys_url + "/" + csm_url );
            if( response.Length != 0 ) {
                if( response.IndexOf( "SECURITYERROR" ) != -1 ) {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                } else {
                    return new JavaScriptSerializer().Deserialize<responseChangedSupplierMeters>( response );
                }
            } else {
                return new responseChangedSupplierMeters();
            }
        }

        public string postData( string request, string url ) {
            if( getST() == false ) {
                return string.Empty;
            }

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create( url );

            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.CachePolicy = new HttpRequestCachePolicy( HttpRequestCacheLevel.NoCacheNoStore );
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = request.Length;
            httpWebRequest.Host = "tys.epias.com.tr";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add( "Charset", "UTF-8" );
            httpWebRequest.Headers.Add( "ecms-service-ticket", st );

            using( StreamWriter streamWriter = new StreamWriter( httpWebRequest.GetRequestStream() ) ) {
                streamWriter.Write( request );
                streamWriter.Flush();
                streamWriter.Close();
            }

            try {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using( StreamReader streamReader = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    return streamReader.ReadToEnd();
                }
            } catch( Exception e ) {
                throw new Exception( e.Message );
            }
        }

        private bool getST() {
            if( getTGT() == false ) {
                return false;
            }

            string request = "service=" + tys_url;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create( tgt + "?format=text" );

            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.CachePolicy = new HttpRequestCachePolicy( HttpRequestCacheLevel.NoCacheNoStore );
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = request.Length;
            httpWebRequest.Host = ( new Uri( tgt ) ).Host;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add( "Charset", "UTF-8" );

            using( StreamWriter streamWriter = new StreamWriter( httpWebRequest.GetRequestStream() ) ) {
                streamWriter.Write( request );
                streamWriter.Flush();
                streamWriter.Close();
            }

            try {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using( StreamReader streamReader = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    st = streamReader.ReadToEnd();

                    return true;
                }
            } catch {
                throw new Exception( "Error on getting ST" );
            }
        }

        private bool getTGT() {
            if( user_name.Length == 0 || user_pass.Length == 0 ) {
                throw new Exception( "EXIST username and / or password is empty." );
            }

            if( swTGT.IsRunning == true && swTGT.Elapsed.TotalMinutes < 44 ) {
                return true;
            }

            string request = "username=" + user_name + "&password=" + user_pass;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create( tgt_url );

            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.CachePolicy = new HttpRequestCachePolicy( HttpRequestCacheLevel.NoCacheNoStore );
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = request.Length;
            httpWebRequest.Host = (new Uri( tgt_url ) ).Host;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add( "Charset", "UTF-8" );

            if( swTGT.IsRunning ) {
                swTGT.Reset();
            } else {
                swTGT.Start();
            }

            using( StreamWriter streamWriter = new StreamWriter( httpWebRequest.GetRequestStream() ) ) {
                streamWriter.Write( request );
                streamWriter.Flush();
                streamWriter.Close();
            }

            try {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using( StreamReader streamReader = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    tgt = httpResponse.GetResponseHeader( "Location" );

                    return true;
                }
            } catch {
                throw new Exception( "Error on getting TGT" );
            }
        }
    }


    /**
     * list-changed-supplier-meters
     * response
     **/
    public class responseChangedSupplierMeters {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
        public rcsmBody body { get; set; }
    }

    public class ChangedSupplierMeter {
        public int newMeterId { get; set; }
        public string newMeterEic { get; set; }
        public string newOrganizationEic { get; set; }
        public string oldOrganizationEic { get; set; }
        public string newCustomerNo { get; set; }
        public string newMeterName { get; set; }
        public string newMeterAddress { get; set; }
        public int newMeterCountyId { get; set; }
        public int newMeterReadingType { get; set; }
        public string newMeterReadingTypeEnum { get; set; }
        public int newProfileSubscriptionGroup { get; set; }
        public double newAverageAnnualConsumption { get; set; }
        public string newDistributionMeterCode { get; set; }
        public string newprofileSubscriptionGroupName { get; set; }
        public string newCity { get; set; }
        public string oldOrganizationCode { get; set; }
        public string newOrganizationCode { get; set; }
    }

    public class rcsmBody {
        public QueryInformation queryInformation { get; set; }
        public List<ChangedSupplierMeter> changedSupplierMeterListResponse { get; set; }
    }

    /**
     * list-changed-supplier-meters
     * request
     **/
    public class requestChangedSupplierMeters {
        public List<Header> header { get; set; }
        public csmBody body { get; set; }

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    public class csmBody {
        public DateTime term { get; set; }
        public string listType { get; set; }
        public Range range { get; set; }

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"listType\":\"" + listType + "\",\"range\":" + range.ToString() + "}";
        }
    }

    /**
     * list-meter-data-configuration
     * response
     **/
    public class responseMeteringDataConfiguration {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
        public rmdcBody body { get; set; }
    }

    public class rmdcBody {
        public QueryInformation queryInformation { get; set; }
        public List<MeterDatas> meterDatas { get; set; }
    }

    public class QueryInformation {
        public int begin { get; set; }
        public int end { get; set; }
        public int count { get; set; }
    }

    public class MeterDatas {
        public UInt32 meterId { get; set; }
        public string meterEic { get; set; }
        public string meterCity { get; set; }
        public DateTime? term { get; set; }
        public DateTime? dataVersion { get; set; }
        public DateTime? confVersion { get; set; }
        public string meterConsumption { get; set; }
        public string meterGeneration { get; set; }
        public string meterLossyConsumption { get; set; }
        public string meterLossyGeneration { get; set; }
        public string readingType { get; set; }
        public string supplierOrganization { get; set; }
        public string meterReadingCompany { get; set; }
        public bool isConfWithdrawalSettlement { get; set; }
        public bool isConfSupplySettlement { get; set; }
        public bool isConfWithdDeducSettlement { get; set; }
        public bool isConfSupplyDeducSettlement { get; set; }
        public bool isRead { get; set; }
    }   

    /**
     * list-meter-data-configuration
     * request
     **/
    public class requestMeterDataConfiguration {
        public List<Header> header;
        public mdcBody body;

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    public class mdcBody {
        private string _meteringReadingType;

        public DateTime term;
        public string meteringReadingType {
            get { return ( _meteringReadingType == "null" ) ? ( "null" ) : ( "\"" + _meteringReadingType + "\"" ); }
            set { this._meteringReadingType = value; }
        }
        public bool pastVersion;
        public Range range;

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"pastVersion\":" + meteringReadingType + ",\"range\":" + range.ToString() + "}";
        }
    }

    /**
     * Common classes
     **/
    public class responseError {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
    }

    public class Header {
        public string key;
        public string value;

        public Header( string key, string value ) {
            this.key = key;
            this.value = value;
        }

        public override string ToString( ) {
            return "{\"key\":\"" + key + "\",\"value\":\"" + value + "\"}";
        }
    }

    public class Range {
        public int begin;
        public int end;

        public override string ToString( ) {
            return "{\"begin\": " + begin + ",\"end\": " + end + "}";
        }
    }

    public static class proGEDIA {
        public static string ToString( DateTime dt ) {
            if( dt >= new DateTime( 2016, 10, 1, 0, 0, 0 ) || ( dt.Year == 2015 && dt.Month == 11 ) ) {
                return dt.ToString( "yyyy-MM-dd\\T00:00:00.000" ) + "+0300";
            } else if( dt.Month > 10 || dt.Month < 4 ) {
                return dt.ToString( "yyyy-MM-dd\\T00:00:00.000" ) + "+0200";
            } else {
                return dt.ToString( "yyyy-MM-dd\\T00:00:00.000" ) + "+0300";
            }
        }
    }

    public class EXISTException : Exception {
        public responseError error = new responseError();

        public EXISTException( string message ) : base( message ) {

        }
    }
}
