using System.Collections.Generic;

namespace RhinoInside.Revit.External.DB.Schemas
{
  public partial class UnitType
  {
    static readonly Dictionary<UnitType, int> map = new Dictionary<UnitType, int>()
    {
      { CandelasPerSquareFoot, -1 }, // DUT_CUSTOM
      { CubicFeetPerMinutePerBritishThermalUnitPerHour, -1 }, // DUT_CUSTOM
      { CubicFeetPerPoundMass, -1 }, // DUT_CUSTOM
      { CubicMetersPerHourCubicMeter, -1 }, // DUT_CUSTOM
      { CubicMetersPerHourSquareMeter, -1 }, // DUT_CUSTOM
      { CubicMetersPerKilogram, -1 }, // DUT_CUSTOM
      { CubicMetersPerWattSecond, -1 }, // DUT_CUSTOM
      { CurrencyPerBritishThermalUnit, -1 }, // DUT_CUSTOM
      { CurrencyPerBritishThermalUnitPerHour, -1 }, // DUT_CUSTOM
      { CurrencyPerSquareFoot, -1 }, // DUT_CUSTOM
      { CurrencyPerSquareMeter, -1 }, // DUT_CUSTOM
      { CurrencyPerWatt, -1 }, // DUT_CUSTOM
      { CurrencyPerWattHour, -1 }, // DUT_CUSTOM
      { Grains, -1 }, // DUT_CUSTOM
      { Grams, -1 }, // DUT_CUSTOM
      { Hectometers, -1 }, // DUT_CUSTOM
      { InchesPerSecond, -1 }, // DUT_CUSTOM
      { JoulesPerKilogram, -1 }, // DUT_CUSTOM
      { KilogramKelvins, -1 }, // DUT_CUSTOM
      { KilogramsPerHour, -1 }, // DUT_CUSTOM
      { KilogramsPerKilogramKelvin, -1 }, // DUT_CUSTOM
      { KilogramsPerMeterHour, -1 }, // DUT_CUSTOM
      { KilogramsPerMeterSecond, -1 }, // DUT_CUSTOM
      { KilogramsPerMinute, -1 }, // DUT_CUSTOM
      { KilogramsPerSecond, -1 }, // DUT_CUSTOM
      { Kilometers, -1 }, // DUT_CUSTOM
      { KilometersPerSecond, -1 }, // DUT_CUSTOM
      { MetersOfWaterColumn, -1 }, // DUT_CUSTOM
      { MetersOfWaterColumnPerMeter, -1 }, // DUT_CUSTOM
      { Miles, -1 }, // DUT_CUSTOM
      { MilesPerSecond, -1 }, // DUT_CUSTOM
      { Milligrams, -1 }, // DUT_CUSTOM
      { MillimetersOfWaterColumn, -1 }, // DUT_CUSTOM
      { MillimetersOfWaterColumnPerMeter, -1 }, // DUT_CUSTOM
      { Nanograms, -1 }, // DUT_CUSTOM
      { NewtonSecondsPerSquareMeter, -1 }, // DUT_CUSTOM
      { Ohms, -1 }, // DUT_CUSTOM
      { Pi, -1 }, // DUT_CUSTOM
      { Poises, -1 }, // DUT_CUSTOM
      { PoundForceSecondsPerSquareFoot, -1 }, // DUT_CUSTOM
      { PoundMassDegreesFahrenheit, -1 }, // DUT_CUSTOM
      { PoundsMassPerHour, -1 }, // DUT_CUSTOM
      { PoundsMassPerMinute, -1 }, // DUT_CUSTOM
      { PoundsMassPerPoundDegreeFahrenheit, -1 }, // DUT_CUSTOM
      { PoundsMassPerSecond, -1 }, // DUT_CUSTOM
      { RevolutionsPerMinute, -1 }, // DUT_CUSTOM
      { RevolutionsPerSecond, -1 }, // DUT_CUSTOM
      { SquareFeetPerSecond, -1 }, // DUT_CUSTOM
      { SquareHectometers, -1 }, // DUT_CUSTOM
      { SquareMetersPerSecond, -1 }, // DUT_CUSTOM
      { SquareYards, -1 }, // DUT_CUSTOM
      { StandardGravity, -1 }, // DUT_CUSTOM
      { Steradians, -1 }, // DUT_CUSTOM
      { ThousandBritishThermalUnitsPerHour, -1 }, // DUT_CUSTOM
      { Turns, -1 }, // DUT_CUSTOM
      { WaterDensity4DegreesCelsius, -1 }, // DUT_CUSTOM
      { WattsPerCubicFootPerMinute, -1 }, // DUT_CUSTOM
      { WattsPerCubicMeterPerSecond, -1 }, // DUT_CUSTOM
      { WattsPerFoot, -1 }, // DUT_CUSTOM
      { WattsPerMeter, -1 }, // DUT_CUSTOM
      { Yards, -1 }, // DUT_CUSTOM
      { StationingSurveyFeet, -1 }, // DUT_CUSTOM
      { Meters, 0 }, // DUT_METERS
      { Centimeters, 1 }, // DUT_CENTIMETERS
      { Millimeters, 2 }, // DUT_MILLIMETERS
      { Feet, 3 }, // DUT_DECIMAL_FEET
      { FeetFractionalInches, 4 }, // DUT_FEET_FRACTIONAL_INCHES
      { FractionalInches, 5 }, // DUT_FRACTIONAL_INCHES
      { Inches, 6 }, // DUT_DECIMAL_INCHES
      { Acres, 7 }, // DUT_ACRES
      { Hectares, 8 }, // DUT_HECTARES
      { MetersCentimeters, 9 }, // DUT_METERS_CENTIMETERS
      { CubicYards, 10 }, // DUT_CUBIC_YARDS
      { SquareFeet, 11 }, // DUT_SQUARE_FEET
      { SquareMeters, 12 }, // DUT_SQUARE_METERS
      { CubicFeet, 13 }, // DUT_CUBIC_FEET
      { CubicMeters, 14 }, // DUT_CUBIC_METERS
      { Degrees, 15 }, // DUT_DECIMAL_DEGREES
      { DegreesMinutes, 16 }, // DUT_DEGREES_AND_MINUTES
      { General, 17 }, // DUT_GENERAL
      { Fixed, 18 }, // DUT_FIXED
      { Percentage, 19 }, // DUT_PERCENTAGE
      { SquareInches, 20 }, // DUT_SQUARE_INCHES
      { SquareCentimeters, 21 }, // DUT_SQUARE_CENTIMETERS
      { SquareMillimeters, 22 }, // DUT_SQUARE_MILLIMETERS
      { CubicInches, 23 }, // DUT_CUBIC_INCHES
      { CubicCentimeters, 24 }, // DUT_CUBIC_CENTIMETERS
      { CubicMillimeters, 25 }, // DUT_CUBIC_MILLIMETERS
      { Liters, 26 }, // DUT_LITERS
      { UsGallons, 27 }, // DUT_GALLONS_US
      { KilogramsPerCubicMeter, 28 }, // DUT_KILOGRAMS_PER_CUBIC_METER
      { PoundsMassPerCubicFoot, 29 }, // DUT_POUNDS_MASS_PER_CUBIC_FOOT
      { PoundsMassPerCubicInch, 30 }, // DUT_POUNDS_MASS_PER_CUBIC_INCH
      { BritishThermalUnits, 31 }, // DUT_BRITISH_THERMAL_UNITS
      { Calories, 32 }, // DUT_CALORIES
      { Kilocalories, 33 }, // DUT_KILOCALORIES
      { Joules, 34 }, // DUT_JOULES
      { KilowattHours, 35 }, // DUT_KILOWATT_HOURS
      { Therms, 36 }, // DUT_THERMS
      { InchesOfWater60DegreesFahrenheitPer100Feet, 37 }, // DUT_INCHES_OF_WATER_PER_100FT
      { PascalsPerMeter, 38 }, // DUT_PASCALS_PER_METER
      { Watts, 39 }, // DUT_WATTS
      { Kilowatts, 40 }, // DUT_KILOWATTS
      { BritishThermalUnitsPerSecond, 41 }, // DUT_BRITISH_THERMAL_UNITS_PER_SECOND
      { BritishThermalUnitsPerHour, 42 }, // DUT_BRITISH_THERMAL_UNITS_PER_HOUR
      { CaloriesPerSecond, 43 }, // DUT_CALORIES_PER_SECOND
      { KilocaloriesPerSecond, 44 }, // DUT_KILOCALORIES_PER_SECOND
      { WattsPerSquareFoot, 45 }, // DUT_WATTS_PER_SQUARE_FOOT
      { WattsPerSquareMeter, 46 }, // DUT_WATTS_PER_SQUARE_METER
      { InchesOfWater60DegreesFahrenheit, 47 }, // DUT_INCHES_OF_WATER
      { Pascals, 48 }, // DUT_PASCALS
      { Kilopascals, 49 }, // DUT_KILOPASCALS
      { Megapascals, 50 }, // DUT_MEGAPASCALS
      { PoundsForcePerSquareInch, 51 }, // DUT_POUNDS_FORCE_PER_SQUARE_INCH
      { InchesOfMercury32DegreesFahrenheit, 52 }, // DUT_INCHES_OF_MERCURY
      { MillimetersOfMercury, 53 }, // DUT_MILLIMETERS_OF_MERCURY
      { Atmospheres, 54 }, // DUT_ATMOSPHERES
      { Bars, 55 }, // DUT_BARS
      { Fahrenheit, 56 }, // DUT_FAHRENHEIT
      { Celsius, 57 }, // DUT_CELSIUS
      { Kelvin, 58 }, // DUT_KELVIN
      { Rankine, 59 }, // DUT_RANKINE
      { FeetPerMinute, 60 }, // DUT_FEET_PER_MINUTE
      { MetersPerSecond, 61 }, // DUT_METERS_PER_SECOND
      { CentimetersPerMinute, 62 }, // DUT_CENTIMETERS_PER_MINUTE
      { CubicFeetPerMinute, 63 }, // DUT_CUBIC_FEET_PER_MINUTE
      { LitersPerSecond, 64 }, // DUT_LITERS_PER_SECOND
      { CubicMetersPerSecond, 65 }, // DUT_CUBIC_METERS_PER_SECOND
      { CubicMetersPerHour, 66 }, // DUT_CUBIC_METERS_PER_HOUR
      { UsGallonsPerMinute, 67 }, // DUT_GALLONS_US_PER_MINUTE
      { UsGallonsPerHour, 68 }, // DUT_GALLONS_US_PER_HOUR
      { Amperes, 69 }, // DUT_AMPERES
      { Kiloamperes, 70 }, // DUT_KILOAMPERES
      { Milliamperes, 71 }, // DUT_MILLIAMPERES
      { Volts, 72 }, // DUT_VOLTS
      { Kilovolts, 73 }, // DUT_KILOVOLTS
      { Millivolts, 74 }, // DUT_MILLIVOLTS
      { Hertz, 75 }, // DUT_HERTZ
      { CyclesPerSecond, 76 }, // DUT_CYCLES_PER_SECOND
      { Lux, 77 }, // DUT_LUX
      { Footcandles, 78 }, // DUT_FOOTCANDLES
      { Footlamberts, 79 }, // DUT_FOOTLAMBERTS
      { CandelasPerSquareMeter, 80 }, // DUT_CANDELAS_PER_SQUARE_METER
      { Candelas, 81 }, // DUT_CANDELAS
      { Lumens, 83 }, // DUT_LUMENS
      { VoltAmperes, 84 }, // DUT_VOLT_AMPERES
      { KilovoltAmperes, 85 }, // DUT_KILOVOLT_AMPERES
      { Horsepower, 86 }, // DUT_HORSEPOWER
      { Newtons, 87 }, // DUT_NEWTONS
      { Dekanewtons, 88 }, // DUT_DECANEWTONS
      { Kilonewtons, 89 }, // DUT_KILONEWTONS
      { Meganewtons, 90 }, // DUT_MEGANEWTONS
      { Kips, 91 }, // DUT_KIPS
      { KilogramsForce, 92 }, // DUT_KILOGRAMS_FORCE
      { TonnesForce, 93 }, // DUT_TONNES_FORCE
      { PoundsForce, 94 }, // DUT_POUNDS_FORCE
      { NewtonsPerMeter, 95 }, // DUT_NEWTONS_PER_METER
      { DekanewtonsPerMeter, 96 }, // DUT_DECANEWTONS_PER_METER
      { KilonewtonsPerMeter, 97 }, // DUT_KILONEWTONS_PER_METER
      { MeganewtonsPerMeter, 98 }, // DUT_MEGANEWTONS_PER_METER
      { KipsPerFoot, 99 }, // DUT_KIPS_PER_FOOT
      { KilogramsForcePerMeter, 100 }, // DUT_KILOGRAMS_FORCE_PER_METER
      { TonnesForcePerMeter, 101 }, // DUT_TONNES_FORCE_PER_METER
      { PoundsForcePerFoot, 102 }, // DUT_POUNDS_FORCE_PER_FOOT
      { NewtonsPerSquareMeter, 103 }, // DUT_NEWTONS_PER_SQUARE_METER
      { DekanewtonsPerSquareMeter, 104 }, // DUT_DECANEWTONS_PER_SQUARE_METER
      { KilonewtonsPerSquareMeter, 105 }, // DUT_KILONEWTONS_PER_SQUARE_METER
      { MeganewtonsPerSquareMeter, 106 }, // DUT_MEGANEWTONS_PER_SQUARE_METER
      { KipsPerSquareFoot, 107 }, // DUT_KIPS_PER_SQUARE_FOOT
      { KilogramsForcePerSquareMeter, 108 }, // DUT_KILOGRAMS_FORCE_PER_SQUARE_METER
      { TonnesForcePerSquareMeter, 109 }, // DUT_TONNES_FORCE_PER_SQUARE_METER
      { PoundsForcePerSquareFoot, 110 }, // DUT_POUNDS_FORCE_PER_SQUARE_FOOT
      { NewtonMeters, 111 }, // DUT_NEWTON_METERS
      { DekanewtonMeters, 112 }, // DUT_DECANEWTON_METERS
      { KilonewtonMeters, 113 }, // DUT_KILONEWTON_METERS
      { MeganewtonMeters, 114 }, // DUT_MEGANEWTON_METERS
      { KipFeet, 115 }, // DUT_KIP_FEET
      { KilogramForceMeters, 116 }, // DUT_KILOGRAM_FORCE_METERS
      { TonneForceMeters, 117 }, // DUT_TONNE_FORCE_METERS
      { PoundForceFeet, 118 }, // DUT_POUND_FORCE_FEET
      { MetersPerKilonewton, 119 }, // DUT_METERS_PER_KILONEWTON
      { FeetPerKip, 120 }, // DUT_FEET_PER_KIP
      { SquareMetersPerKilonewton, 121 }, // DUT_SQUARE_METERS_PER_KILONEWTON
      { SquareFeetPerKip, 122 }, // DUT_SQUARE_FEET_PER_KIP
      { CubicMetersPerKilonewton, 123 }, // DUT_CUBIC_METERS_PER_KILONEWTON
      { CubicFeetPerKip, 124 }, // DUT_CUBIC_FEET_PER_KIP
      { InverseKilonewtons, 125 }, // DUT_INV_KILONEWTONS
      { InverseKips, 126 }, // DUT_INV_KIPS
      { FeetOfWater39_2DegreesFahrenheitPer100Feet, 127 }, // DUT_FEET_OF_WATER_PER_100FT
      { FeetOfWater39_2DegreesFahrenheit, 128 }, // DUT_FEET_OF_WATER
      { PascalSeconds, 129 }, // DUT_PASCAL_SECONDS
      { PoundsMassPerFootSecond, 130 }, // DUT_POUNDS_MASS_PER_FOOT_SECOND
      { Centipoises, 131 }, // DUT_CENTIPOISES
      { FeetPerSecond, 132 }, // DUT_FEET_PER_SECOND
      { KipsPerSquareInch, 133 }, // DUT_KIPS_PER_SQUARE_INCH
      { KilonewtonsPerCubicMeter, 134 }, // DUT_KILONEWTONS_PER_CUBIC_METER
      { PoundsForcePerCubicFoot, 135 }, // DUT_POUNDS_FORCE_PER_CUBIC_FOOT
      { KipsPerCubicInch, 136 }, // DUT_KIPS_PER_CUBIC_INCH
      { InverseDegreesFahrenheit, 137 }, // DUT_INV_FAHRENHEIT
      { InverseDegreesCelsius, 138 }, // DUT_INV_CELSIUS
      { NewtonMetersPerMeter, 139 }, // DUT_NEWTON_METERS_PER_METER
      { DekanewtonMetersPerMeter, 140 }, // DUT_DECANEWTON_METERS_PER_METER
      { KilonewtonMetersPerMeter, 141 }, // DUT_KILONEWTON_METERS_PER_METER
      { MeganewtonMetersPerMeter, 142 }, // DUT_MEGANEWTON_METERS_PER_METER
      { KipFeetPerFoot, 143 }, // DUT_KIP_FEET_PER_FOOT
      { KilogramForceMetersPerMeter, 144 }, // DUT_KILOGRAM_FORCE_METERS_PER_METER
      { TonneForceMetersPerMeter, 145 }, // DUT_TONNE_FORCE_METERS_PER_METER
      { PoundForceFeetPerFoot, 146 }, // DUT_POUND_FORCE_FEET_PER_FOOT
      { PoundsMassPerFootHour, 147 }, // DUT_POUNDS_MASS_PER_FOOT_HOUR
      { KipsPerInch, 148 }, // DUT_KIPS_PER_INCH
      { KipsPerCubicFoot, 149 }, // DUT_KIPS_PER_CUBIC_FOOT
      { KipFeetPerDegree, 150 }, // DUT_KIP_FEET_PER_DEGREE
      { KilonewtonMetersPerDegree, 151 }, // DUT_KILONEWTON_METERS_PER_DEGREE
      { KipFeetPerDegreePerFoot, 152 }, // DUT_KIP_FEET_PER_DEGREE_PER_FOOT
      { KilonewtonMetersPerDegreePerMeter, 153 }, // DUT_KILONEWTON_METERS_PER_DEGREE_PER_METER
      { WattsPerSquareMeterKelvin, 154 }, // DUT_WATTS_PER_SQUARE_METER_KELVIN
      { BritishThermalUnitsPerHourSquareFootDegreeFahrenheit, 155 }, // DUT_BRITISH_THERMAL_UNITS_PER_HOUR_SQUARE_FOOT_FAHRENHEIT
      { CubicFeetPerMinuteSquareFoot, 156 }, // DUT_CUBIC_FEET_PER_MINUTE_SQUARE_FOOT
      { LitersPerSecondSquareMeter, 157 }, // DUT_LITERS_PER_SECOND_SQUARE_METER
      { RatioTo10, 158 }, // DUT_RATIO_10
      { RatioTo12, 159 }, // DUT_RATIO_12
      { SlopeDegrees, 160 }, // DUT_SLOPE_DEGREES
      { RiseDividedBy12Inches, 161 }, // DUT_RISE_OVER_INCHES
      { RiseDividedBy1Foot, 162 }, // DUT_RISE_OVER_FOOT
      { RiseDividedBy1000Millimeters, 163 }, // DUT_RISE_OVER_MMS
      { WattsPerCubicFoot, 164 }, // DUT_WATTS_PER_CUBIC_FOOT
      { WattsPerCubicMeter, 165 }, // DUT_WATTS_PER_CUBIC_METER
      { BritishThermalUnitsPerHourSquareFoot, 166 }, // DUT_BRITISH_THERMAL_UNITS_PER_HOUR_SQUARE_FOOT
      { BritishThermalUnitsPerHourCubicFoot, 167 }, // DUT_BRITISH_THERMAL_UNITS_PER_HOUR_CUBIC_FOOT
      { TonsOfRefrigeration, 168 }, // DUT_TON_OF_REFRIGERATION
      { CubicFeetPerMinuteCubicFoot, 169 }, // DUT_CUBIC_FEET_PER_MINUTE_CUBIC_FOOT
      { LitersPerSecondCubicMeter, 170 }, // DUT_LITERS_PER_SECOND_CUBIC_METER
      { CubicFeetPerMinuteTonOfRefrigeration, 171 }, // DUT_CUBIC_FEET_PER_MINUTE_TON_OF_REFRIGERATION
      { LitersPerSecondKilowatt, 172 }, // DUT_LITERS_PER_SECOND_KILOWATTS
      { SquareFeetPerTonOfRefrigeration, 173 }, // DUT_SQUARE_FEET_PER_TON_OF_REFRIGERATION
      { SquareMetersPerKilowatt, 174 }, // DUT_SQUARE_METERS_PER_KILOWATTS
      { Currency, 175 }, // DUT_CURRENCY
      { LumensPerWatt, 176 }, // DUT_LUMENS_PER_WATT
      { SquareFeetPer1000BritishThermalUnitsPerHour, 177 }, // DUT_SQUARE_FEET_PER_THOUSAND_BRITISH_THERMAL_UNITS_PER_HOUR
      { KilonewtonsPerSquareCentimeter, 178 }, // DUT_KILONEWTONS_PER_SQUARE_CENTIMETER
      { NewtonsPerSquareMillimeter, 179 }, // DUT_NEWTONS_PER_SQUARE_MILLIMETER
      { KilonewtonsPerSquareMillimeter, 180 }, // DUT_KILONEWTONS_PER_SQUARE_MILLIMETER
      { RiseDividedBy120Inches, 181 }, // DUT_RISE_OVER_120_INCHES
      { OneToRatio, 182 }, // DUT_1_RATIO
      { RiseDividedBy10Feet, 183 }, // DUT_RISE_OVER_10_FEET
      { HourSquareFootDegreesFahrenheitPerBritishThermalUnit, 184 }, // DUT_HOUR_SQUARE_FOOT_FAHRENHEIT_PER_BRITISH_THERMAL_UNIT
      { SquareMeterKelvinsPerWatt, 185 }, // DUT_SQUARE_METER_KELVIN_PER_WATT
      { BritishThermalUnitsPerDegreeFahrenheit, 186 }, // DUT_BRITISH_THERMAL_UNIT_PER_FAHRENHEIT
      { JoulesPerKelvin, 187 }, // DUT_JOULES_PER_KELVIN
      { KilojoulesPerKelvin, 188 }, // DUT_KILOJOULES_PER_KELVIN
      { Kilograms, 189 }, // DUT_KILOGRAMS_MASS
      { Tonnes, 190 }, // DUT_TONNES_MASS
      { PoundsMass, 191 }, // DUT_POUNDS_MASS
      { MetersPerSecondSquared, 192 }, // DUT_METERS_PER_SECOND_SQUARED
      { KilometersPerSecondSquared, 193 }, // DUT_KILOMETERS_PER_SECOND_SQUARED
      { InchesPerSecondSquared, 194 }, // DUT_INCHES_PER_SECOND_SQUARED
      { FeetPerSecondSquared, 195 }, // DUT_FEET_PER_SECOND_SQUARED
      { MilesPerSecondSquared, 196 }, // DUT_MILES_PER_SECOND_SQUARED
      { FeetToTheFourthPower, 197 }, // DUT_FEET_TO_THE_FOURTH_POWER
      { InchesToTheFourthPower, 198 }, // DUT_INCHES_TO_THE_FOURTH_POWER
      { MillimetersToTheFourthPower, 199 }, // DUT_MILLIMETERS_TO_THE_FOURTH_POWER
      { CentimetersToTheFourthPower, 200 }, // DUT_CENTIMETERS_TO_THE_FOURTH_POWER
      { MetersToTheFourthPower, 201 }, // DUT_METERS_TO_THE_FOURTH_POWER
      { FeetToTheSixthPower, 202 }, // DUT_FEET_TO_THE_SIXTH_POWER
      { InchesToTheSixthPower, 203 }, // DUT_INCHES_TO_THE_SIXTH_POWER
      { MillimetersToTheSixthPower, 204 }, // DUT_MILLIMETERS_TO_THE_SIXTH_POWER
      { CentimetersToTheSixthPower, 205 }, // DUT_CENTIMETERS_TO_THE_SIXTH_POWER
      { MetersToTheSixthPower, 206 }, // DUT_METERS_TO_THE_SIXTH_POWER
      { SquareFeetPerFoot, 207 }, // DUT_SQUARE_FEET_PER_FOOT
      { SquareInchesPerFoot, 208 }, // DUT_SQUARE_INCHES_PER_FOOT
      { SquareMillimetersPerMeter, 209 }, // DUT_SQUARE_MILLIMETERS_PER_METER
      { SquareCentimetersPerMeter, 210 }, // DUT_SQUARE_CENTIMETERS_PER_METER
      { SquareMetersPerMeter, 211 }, // DUT_SQUARE_METERS_PER_METER
      { KilogramsPerMeter, 212 }, // DUT_KILOGRAMS_MASS_PER_METER
      { PoundsMassPerFoot, 213 }, // DUT_POUNDS_MASS_PER_FOOT
      { Radians, 214 }, // DUT_RADIANS
      { Gradians, 215 }, // DUT_GRADS
      { RadiansPerSecond, 216 }, // DUT_RADIANS_PER_SECOND
      { Milliseconds, 217 }, // DUT_MILISECONDS
      { Seconds, 218 }, // DUT_SECONDS
      { Minutes, 219 }, // DUT_MINUTES
      { Hours, 220 }, // DUT_HOURS
      { KilometersPerHour, 221 }, // DUT_KILOMETERS_PER_HOUR
      { MilesPerHour, 222 }, // DUT_MILES_PER_HOUR
      { Kilojoules, 223 }, // DUT_KILOJOULES
      { KilogramsPerSquareMeter, 224 }, // DUT_KILOGRAMS_MASS_PER_SQUARE_METER
      { PoundsMassPerSquareFoot, 225 }, // DUT_POUNDS_MASS_PER_SQUARE_FOOT
      { WattsPerMeterKelvin, 226 }, // DUT_WATTS_PER_METER_KELVIN
      { JoulesPerGramDegreeCelsius, 227 }, // DUT_JOULES_PER_GRAM_CELSIUS
      { JoulesPerGram, 228 }, // DUT_JOULES_PER_GRAM
      { NanogramsPerPascalSecondSquareMeter, 229 }, // DUT_NANOGRAMS_PER_PASCAL_SECOND_SQUARE_METER
      { OhmMeters, 230 }, // DUT_OHM_METERS
      { BritishThermalUnitsPerHourFootDegreeFahrenheit, 231 }, // DUT_BRITISH_THERMAL_UNITS_PER_HOUR_FOOT_FAHRENHEIT
      { BritishThermalUnitsPerPoundDegreeFahrenheit, 232 }, // DUT_BRITISH_THERMAL_UNITS_PER_POUND_FAHRENHEIT
      { BritishThermalUnitsPerPound, 233 }, // DUT_BRITISH_THERMAL_UNITS_PER_POUND
      { GrainsPerHourSquareFootInchMercury, 234 }, // DUT_GRAINS_PER_HOUR_SQUARE_FOOT_INCH_MERCURY
      { PerMille, 235 }, // DUT_PER_MILLE
      { Decimeters, 236 }, // DUT_DECIMETERS
      { JoulesPerKilogramDegreeCelsius, 237 }, // DUT_JOULES_PER_KILOGRAM_CELSIUS
      { MicrometersPerMeterDegreeCelsius, 238 }, // DUT_MICROMETERS_PER_METER_CELSIUS
      { MicroinchesPerInchDegreeFahrenheit, 239 }, // DUT_MICROINCHES_PER_INCH_FAHRENHEIT
      { UsTonnesMass, 240 }, // DUT_USTONNES_MASS
      { UsTonnesForce, 241 }, // DUT_USTONNES_FORCE
      { LitersPerMinute, 242 }, // DUT_LITERS_PER_MINUTE
      { FahrenheitInterval, 243 }, // DUT_FAHRENHEIT_DIFFERENCE
      { CelsiusInterval, 244 }, // DUT_CELSIUS_DIFFERENCE
      { KelvinInterval, 245 }, // DUT_KELVIN_DIFFERENCE
      { RankineInterval, 246 }, // DUT_RANKINE_DIFFERENCE
      { StationingMeters, 247 }, // DUT_STATIONING_METERS
      { StationingFeet, 248 }, // DUT_STATIONING_FEET
      { CubicFeetPerHour, 249 }, // DUT_CUBIC_FEET_PER_HOUR
      { LitersPerHour, 250 }, // DUT_LITERS_PER_HOUR
      { RatioTo1, 251 }, // DUT_RATIO_TO_1
      { UsSurveyFeet, 605 }, // DUT_DECIMAL_US_SURVEY_FEET
    };
  }
}