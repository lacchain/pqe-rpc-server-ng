using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.OqsdotNet
{

    //if(test.ToString().Replace("_","_").equals(someValue)) { /* some stuff */ }   need to use underscores enum as int does not allow hyphen
    public enum SupportedAlgorithmsEnum
    {
        /** Algorithm identifier for BIKE1_L13_CPA KEM. */
 BIKE1_L1_CPA = 111,
        /** Algorithm identifier for BIKE1_L3_CPA KEM. */
 BIKE1_L3_CPA = 112,

        /** Algorithm identifier for BIKE1_L1_FO KEM. */
BIKE1_L1_FO = 113,
        /** Algorithm identifier for BIKE1_L3_FO KEM. */
BIKE1_L3_FO=114,

        /** Algorithm identifier for Classic_McEliece_348864 KEM. */
Classic_McEliece_348864 = 322,
        /** Algorithm identifier for Classic_McEliece_348864f KEM. */
Classic_McEliece_348864f =323,
        /** Algorithm identifier for Classic_McEliece_460896 KEM. */
Classic_McEliece_460896 = 324,
        /** Algorithm identifier for Classic_McEliece_460896f KEM. */
Classic_McEliece_460896f=325,
        /** Algorithm identifier for Classic_McEliece_6688128 KEM. */
Classic_McEliece_6688128=326,
        /** Algorithm identifier for Classic_McEliece_6688128f KEM. */
Classic_McEliece_6688128f=327,
        /** Algorithm identifier for Classic_McEliece_6960119 KEM. */
Classic_McEliece_6960119=328,
        /** Algorithm identifier for Classic_McEliece_6960119f KEM. */
Classic_McEliece_6960119f=329,
        /** Algorithm identifier for Classic_McEliece_8192128 KEM. */
Classic_McEliece_8192128=330,
        /** Algorithm identifier for Classic_McEliece_8192128f KEM. */
Classic_McEliece_8192128f=331,
        /** Algorithm identifier for HQC_128_1_CCA2 KEM. */
HQC_128_1_CCA2=350,
        /** Algorithm identifier for HQC_192_1_CCA2 KEM. */
HQC_192_1_CCA2=351,
        /** Algorithm identifier for HQC_192_2_CCA2 KEM. */
HQC_192_2_CCA2=352,
        /** Algorithm identifier for HQC_256_1_CCA2 KEM. */
HQC_256_1_CCA2=353,
        /** Algorithm identifier for HQC_256_2_CCA2 KEM. */
HQC_256_2_CCA2=354,
        /** Algorithm identifier for HQC_256_3_CCA2 KEM. */
HQC_256_3_CCA2=355,
        /** Algorithm identifier for Kyber512 KEM. */
Kyber512= 380,
        /** Algorithm identifier for Kyber768 KEM. */
Kyber768=381,
        /** Algorithm identifier for Kyber1024 KEM. */
Kyber1024=382,
        /** Algorithm identifier for Kyber512_90s KEM. */
Kyber512_90s=383,
        /** Algorithm identifier for Kyber768_90s KEM. */
Kyber768_90s=384,
        /** Algorithm identifier for Kyber1024_90s KEM. */
Kyber1024_90s=385,
        /** Algorithm identifier for NTRU_HPS_2048_509 KEM. */
NTRU_HPS_2048_509=390,
        /** Algorithm identifier for NTRU_HPS_2048_677 KEM. */
NTRU_HPS_2048_677=391,
        /** Algorithm identifier for NTRU_HPS_4096_821 KEM. */
NTRU_HPS_4096_821=392,
        /** Algorithm identifier for NTRU_HRSS_701 KEM. */
NTRU_HRSS_701=393,
        /** Algorithm identifier for LightSaber_KEM KEM. */
LightSaber_KEM=400,
        /** Algorithm identifier for Saber_KEM KEM. */
Saber_KEM=401,
        /** Algorithm identifier for FireSaber_KEM KEM. */
FireSaber_KEM=402,

        /** Algorithm identifier for FrodoKEM_640_AES KEM. */
FrodoKEM_640_AES=222,
        /** Algorithm identifier for FrodoKEM_640_SHAKE KEM. */
FrodoKEM_640_SHAKE=223,
        /** Algorithm identifier for FrodoKEM_976_AES KEM. */
FrodoKEM_976_AES=224,
        /** Algorithm identifier for FrodoKEM_976_SHAKE KEM. */
FrodoKEM_976_SHAKE=225,
        /** Algorithm identifier for FrodoKEM_1344_AES KEM. */
FrodoKEM_1344_AES=226,
        /** Algorithm identifier for FrodoKEM_1344_SHAKE KEM. */
FrodoKEM_1344_SHAKE=227,
        /** Algorithm identifier for SIDH p434 KEM. */
SIDH_p434=250,
        /** Algorithm identifier for SIDH p434 compressed KEM. */
SIDH_p434_compressed=251,
        /** Algorithm identifier for SIDH p503 KEM. */
SIDH_p503=252,
        /** Algorithm identifier for SIDH p503 compressed KEM. */
SIDH_p503_compressed=253,
        /** Algorithm identifier for SIDH p610 KEM. */
SIDH_p610=254,
        /** Algorithm identifier for SIDH p610 compressed KEM. */
SIDH_p610_compressed=255,
        /** Algorithm identifier for SIDH p751 KEM. */
SIDH_p751=256,
        /** Algorithm identifier for SIDH p751 compressed KEM. */
SIDH_p751_compressed=257,
        /** Algorithm identifier for SIKE p434 KEM. */
SIKE_p434=258,
        /** Algorithm identifier for SIKE p434 compressed KEM. */
SIKE_p434_compressed=259,
        /** Algorithm identifier for SIKE p503 KEM. */
SIKE_p503=260,
        /** Algorithm identifier for SIKE p503 compressed KEM. */
SIKE_p503_compressed=261,
        /** Algorithm identifier for SIKE p610 KEM. */
SIKE_p610=262,
        /** Algorithm identifier for SIKE p610 compressed KEM. */
SIKE_p610_compressed=263,
        /** Algorithm identifier for SIKE p751 KEM. */
SIKE_p751=264,
        /** Algorithm identifier for SIKE p751 compressed KEM. */
SIKE_p751_compressed=265,


            //signatures


DILITHIUM_2=400,
DILITHIUM_3=401,
DILITHIUM_4=402,
Falcon_512=403,  //falcon-512
Falcon_1024=404,
picnic_L1_FS=405,
picnic_L1_UR=406,
picnic_L1_full=407,
picnic_L3_FS=408,
picnic_L3_UR=409,
picnic_L3_full=410,
picnic_L5_FS=411,
picnic_L5_UR=412,
picnic_L5_full=413,
picnic3_L1=414,
picnic3_L3=415,
picnic3_L5=416

    }
}
