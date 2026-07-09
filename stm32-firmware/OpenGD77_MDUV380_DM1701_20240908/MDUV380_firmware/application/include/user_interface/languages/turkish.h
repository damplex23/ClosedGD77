/* -*- coding: binary; -*- */
/*
 * Copyright (C) 2019-2024 Roger Clark, VK3KYY / G4KYF
 *
 *
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
 *    in the documentation and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 *
 * 4. Use of this source code or binary releases for commercial purposes is strictly forbidden. This includes, without limitation,
 *    incorporation in a commercial product or incorporation into a product or project which allows commercial use.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 */
/*
 * Translators: TA5AYX/TA2RU
 * Last updated: 2023/08/03
 *
 * Rev: 1.2
 */
#ifndef USER_INTERFACE_LANGUAGES_TURKISH_H_
#define USER_INTERFACE_LANGUAGES_TURKISH_H_
/********************************************************************
 *
 * VERY IMPORTANT.
 * This file should not be saved with UTF-8 encoding
 * Use Notepad++ on Windows with ANSI encoding
 * or emacs on Linux with windows-1252-unix encoding
 *
 ********************************************************************/
#if defined(PLATFORM_GD77) || defined(PLATFORM_GD77S) || defined(PLATFORM_DM1801) || defined(PLATFORM_DM1801A) || defined(PLATFORM_RD5R)
__attribute__((section(".upper_text")))
#endif
const stringsTable_t turkishLanguage =
{
.magicNumber                            = { LANGUAGE_TAG_MAGIC_NUMBER, LANGUAGE_TAG_VERSION },
.LANGUAGE_NAME 						= "Türkçe", // MaxLen: 16
.menu								= "Menü", // MaxLen: 16
.credits							= "Yap“mc“lar", // MaxLen: 16
.zone								= "Bölge", // MaxLen: 16
.rssi								= "RSSI", // MaxLen: 16
.battery							= "Batarya", // MaxLen: 16
.contacts							= "Ki–iler", // MaxLen: 16
.last_heard							= "Son Duyulanlar", // MaxLen: 16
.firmware_info						= "Yaz“l“m Bilgisi", // MaxLen: 16
.options							= "Seçenekler", // MaxLen: 16
.display_options					= "Ekran Seç.", // MaxLen: 16
.sound_options						= "Ses Seç.", // MaxLen: 16
.channel_details					= "Kanal Ayr“nt“s“", // MaxLen: 16
.language							= "Dil", // MaxLen: 16
.new_contact						= "Yeni Ki–i", // MaxLen: 16
.dmr_contacts						= "DMR Ki–ileri", // MaxLen: 16
.contact_details					= "Ki–i Ayr“nt“lar“", // MaxLen: 16
.hotspot_mode						= "Hotspot", // MaxLen: 16
.built								= "Sürüm", // MaxLen: 16
.zones								= "Bölgeler", // MaxLen: 16
.keypad								= "Tu–lar", // MaxLen: 12 (with .ptt)
.ptt								= "Mandal", // MaxLen: 12 (with .keypad)
.locked								= "Kilitli", // MaxLen: 15
.press_sk2_plus_star				= "SK2 + *'a bas“n", // MaxLen: 16
.to_unlock							= "Açmak için", // MaxLen: 16
.unlocked							= "Kilit Aç“k", // MaxLen: 15
.power_off							= "Kapan“yor...", // MaxLen: 16
.error								= "HATA", // MaxLen: 8
.rx_only							= "Sadece Rx", // MaxLen: 14
.out_of_band						= "BANT DI•INDA", // MaxLen: 14
.timeout							= "TIMEOUT", // MaxLen: 8
.tg_entry							= "TG Giri–i", // MaxLen: 15
.pc_entry							= "PC Giri–i", // MaxLen: 15
.user_dmr_id						= "Ki–isel DMR ID", // MaxLen: 15
.contact 							= "Ki–i", // MaxLen: 15
.accept_call						= "Ça˜r“ kabul?", // MaxLen: 16
.private_call						= "Özel Ça˜r“", // MaxLen: 16
.squelch							= "Susturma", // MaxLen: 8
.quick_menu 						= "H“zl“ Menü", // MaxLen: 16
.filter								= "Filtre", // MaxLen: 7 (with ':' + settings: .none, "CC", "CC,TS", "CC,TS,TG")
.all_channels						= "Tüm Kanallar", // MaxLen: 16
.gotoChannel						= "Kanala Git",  // MaxLen: 11 (" 1024")
.scan								= "Tara", // MaxLen: 16
.channelToVfo						= "Kanal --> VFO", // MaxLen: 16
.vfoToChannel						= "VFO --> Kanal", // MaxLen: 16
.vfoToNewChannel					= "VFO --> Y. Kan.", // MaxLen: 16
.group								= "Konu–ma Gr.", // MaxLen: 16 (with .type)
.private							= "Özel Ça˜r“", // MaxLen: 16 (with .type)
.all								= "Tüm Ça˜r“lar", // MaxLen: 16 (with .type)
.type								= "Tür", // MaxLen: 16 (with .type)
.timeSlot							= "Zaman Dilimi", // MaxLen: 16 (plus ':' and  .none, '1' or '2')
.none								= "Yok", // MaxLen: 16 (with .timeSlot, "Rx CTCSS:" and ""Tx CTCSS:", .filter and .mode )
.contact_saved						= "Ki–i Kaydedildi", // MaxLen: 16
.duplicate							= "Benzer Kay“t!", // MaxLen: 16
.tg									= "TG",  // MaxLen: 8
.pc									= "PC", // MaxLen: 8
.ts									= "TS", // MaxLen: 8
.mode								= "Mod",  // MaxLen: 12
.colour_code						= "Renk Kodu", // MaxLen: 16 (with ':' * .n_a)
.n_a								= "N/A",// MaxLen: 16 (with ':' * .colour_code)
.bandwidth							= "BG", // MaxLen: 16 (with : + .n_a, "25kHz" or "12.5kHz")–", // MaxLen: 16 (with ':' + .n_a, "25kHz" or "12.5kHz")
.stepFreq							= "Ad“m", // MaxLen: 7 (with ':' + xx.xxkHz fitted)
.tot								= "TX Süresi", // MaxLen: 16 (with ':' + .off or 15..3825)
.off								= "Kapal“", // MaxLen: 16 (with ':' + .timeout_beep, .band_limits)
.zone_skip							= "Bölge Atla", // MaxLen: 16 (with ':' + .yes or .no)
.all_skip							= "Hepsi Atla", // MaxLen: 16 (with ':' + .yes or .no)
.yes								= "Evet", // MaxLen: 16 (with ':' + .zone_skip, .all_skip)
.no									= "Hay“r", // MaxLen: 16 (with ':' + .zone_skip, .all_skip)
.tg_list							= "TG List.", // MaxLen: 16 (with ':' and codeplug group name)
.on									= "Aç“k", // MaxLen: 16 (with ':' + .band_limits)
.timeout_beep						= "Z.A–“m“ Tonu", // MaxLen: 16 (with ':' + .n_a or 5..20 + 's')
.list_full							= "Liste dolu",
.dmr_cc_scan						= "CC Tara", // MaxLen: 12 (with ':' + settings: .on or .off)
.band_limits						= "Bant S“n“r“", // MaxLen: 16 (with ':' + .on or .off)
.beep_volume						= "Uyar“ Sesi", // MaxLen: 16 (with ':' + -24..6 + 'dB')
.dmr_mic_gain						= "DMR Mik.", // MaxLen: 16 (with ':' + -33..12 + 'dB')
.fm_mic_gain						= "FM Mik.", // MaxLen: 16 (with ':' + 0..31)
.key_long							= "Tu– Uzun", // MaxLen: 11 (with ':' + x.xs fitted)
.key_repeat							= "Tu– Tekr.", // MaxLen: 11 (with ':' + x.xs fitted)
.dmr_filter_timeout					= "Filt. Süresi", // MaxLen: 16 (with ':' + 1..90 + 's')
.brightness							= "Parlakl“k", // MaxLen: 16 (with ':' + 0..100 + '%')
.brightness_off						= "Min. Parlak", // MaxLen: 16 (with ':' + 0..100 + '%')
.contrast							= "Kontrast", // MaxLen: 16 (with ':' + 12..30)
.screen_invert						= "Ters", // MaxLen: 16
.screen_normal						= "Normal", // MaxLen: 16
.backlight_timeout					= "Zamana–“m“", // MaxLen: 16 (with ':' + .no to 30s)
.scan_delay							= "Tarama Gec.", // MaxLen: 16 (with ':' + 1..30 + 's')
.yes___in_uppercase					= "EVET", // MaxLen: 8 (choice above green/red buttons)
.no___in_uppercase					= "HAYIR", // MaxLen: 8 (choice above green/red buttons)
.DISMISS							= "REDDET", // MaxLen: 8 (choice above green/red buttons)
.scan_mode							= "Tarama Modu", // MaxLen: 16 (with ':' + .hold, .pause or .stop)
.hold								= "Dur", // MaxLen: 16 (with ':' + .scan_mode)
.pause								= "Bekle", // MaxLen: 16 (with ':' + .scan_mode)
.list_empty							= "Liste Bo–", // MaxLen: 16
.delete_contact_qm					= "Ki–iyi Sil?", // MaxLen: 16
.contact_deleted					= "Ki–i silindi", // MaxLen: 16
.contact_used						= "Ki–i kullan“mda", // MaxLen: 16
.in_tg_list							= "TG listesinde", // MaxLen: 16
.select_tx							= "TX seçin", // MaxLen: 16
.edit_contact						= "Ki–iyi Düzenle", // MaxLen: 16
.delete_contact						= "Ki–iyi Sil", // MaxLen: 16
.group_call							= "Grup Ça˜r“s“", // MaxLen: 16
.all_call							= "Tüm Ça˜r“lar", // MaxLen: 16
.tone_scan							= "Ton Tarama", // MaxLen: 16
.low_battery						= "DÜ•ÜK BATARYA !", // MaxLen: 16
.Auto								= "Otomatik", // MaxLen 16 (with .mode + ':')
.manual								= "Manuel",  // MaxLen 16 (with .mode + ':')
.ptt_toggle							= "Mandal", // MaxLen 16 (with ':' + .on or .off)
.private_call_handling				= "PC izin", // MaxLen 16 (with ':' + .on or .off)
.stop								= "Dur", // Maxlen 16 (with ':' + .scan_mode)
.one_line							= "1 sat“r", // MaxLen 16 (with ':' + .contact)
.two_lines							= "2 sat“r", // MaxLen 16 (with ':' + .contact)
.new_channel						= "Yeni Kanal", // MaxLen: 16, leave room for a space and four channel digits after
.priority_order						= "Öncelik", // MaxLen 16 (with ':' + 'Cc/DB/TA')
.dmr_beep							= "DMR Uyar“", // MaxLen 16 (with ':' + .star/.stop/.both/.none)
.start								= "Ba–la", // MaxLen 16 (with ':' + .dmr_beep)
.both								= "”kisi de", // MaxLen 16 (with ':' + .dmr_beep)
.vox_threshold                      = "VOX E–i˜i", // MaxLen 16 (with ':' + .off or 1..30)
.vox_tail                           = "VOX Kuy.", // MaxLen 16 (with ':' + .n_a or '0.0s')
.audio_prompt						= "Ses Kom.",// Maxlen 16 (with ':' + .silent, .beep or .voice_prompt_level_1)
.silent                             = "Sessiz", // Maxlen 16 (with : + audio_prompt)
.rx_beep							= "RX Uyar“", // MaxLen 16 (with ':' + .carrier/.talker/.both/.none)
.beep								= "Uyar“", // Maxlen 16 (with : + audio_prompt)
.voice_prompt_level_1				= "Ses", // Maxlen 16 (with : + audio_prompt, satellite "mode")
.transmitTalkerAliasTS1				= "TA Tx TS1", // Maxlen 16 (with : + .on or .off)
.squelch_VHF						= "VHF Susturma",// Maxlen 16 (with : + XX%)
.squelch_220						= "220 Susturma",// Maxlen 16 (with : + XX%)
.squelch_UHF						= "UHF Susturma", // Maxlen 16 (with : + XX%)
.display_screen_invert 				= "Renk" , // Maxlen 16 (with : + .screen_normal or .screen_invert)
.openGD77 							= "OpenGD77",// Do not translate
.talkaround 						= "Yak“n Kon.", // Maxlen 16 (with ':' + .on , .off or .n_a)
.APRS 								= "APRS", // Maxlen 16 (with : + .transmitTalkerAliasTS1 or transmitTalkerAliasTS2)
.no_keys 							= "Tu–lar yok", // Maxlen 16 (with : + audio_prompt)
.gitCommit							= "Git i–leme",
.voice_prompt_level_2				= "Ses L2", // Maxlen 16 (with : + audio_prompt)
.voice_prompt_level_3				= "Ses L3", // Maxlen 16 (with : + audio_prompt)
.dmr_filter							= "DMR Filt.",// MaxLen: 12 (with ':' + settings: "TG" or "Ct" or "TGL")
.talker								= "Konu–mac“",
.dmr_ts_filter						= "TS Filt.", // MaxLen: 12 (with ':' + settings: .on or .off)
.dtmf_contact_list					= "FM DTMF Ki–ileri", // Maxlen: 16
.channel_power						= "Kan. Gücü", //Displayed as "Ch Power:" + .from_master or "Ch Power:"+ power text e.g. "Power:500mW" . Max total length 16
.from_master						= "Ana",// Displayed if per-channel power is not enabled  the .channel_power
.set_quickkey						= "H“zl“ Tu– Ata", // MaxLen: 16
.dual_watch							= "Çift Takip", // MaxLen: 16
.info								= "Bilgi", // MaxLen: 16 (with ':' + .off or.ts or .pwr or .both)
.pwr								= "Güç",
.user_power							= "Kul. Gücü",
.temperature						= "S“cakl“k", // MaxLen: 16 (with ':' + .celcius or .fahrenheit)
.celcius							= "°C",
.seconds							= "seconds",
.radio_info							= "Radyo Bilgileri",
.temperature_calibration			= "S“c. Kal.",
.pin_code							= "Pin Kodu",
.please_confirm						= "Do˜rulay“n", // MaxLen: 15
.vfo_freq_bind_mode					= "Frekans Ba˜lama",
.overwrite_qm						= "Üzerine Yaz ?", //Maxlen: 14 chars
.eco_level							= "Eko. Seviyesi",
.buttons							= "Tu–lar",
.leds								= "LEDler",
.scan_dwell_time					= "Tarama Bek.",
.battery_calibration				= "Bat. Kal.",
.low								= "Dü–ük",
.high								= "Yüksek",
.dmr_id								= "DMR ID",
.scan_on_boot						= "Aç. Tara",
.dtmf_entry							= "DTMF Giri–i",
.name								= "Ad",
.carrier							= "Ta–“y“c“",
.zone_empty 						= "Bölge Bo–", // Maxlen: 12 chars.
.time								= "Zaman",
.uptime								= "Çal“–ma Süresi",
.hours								= "Saat",
.minutes							= "Dakika",
.satellite							= "Uydu",
.alarm_time							= "Alarm Zaman“",
.location							= "Konum",
.date								= "Tarih",
.timeZone							= "Zaman Dilimi",
.suspend							= "Ask“ya Al",
.pass								= "Geçi–", // For satellite screen
.elevation							= "Yük",
.azimuth							= "Az",
.inHHMMSS							= "içinde",
.predicting							= "Güncelleniyor",
.maximum							= "Maks",
.satellite_short					= "Uydu",
.local								= "Yerel",
.UTC								= "UTC",
.symbols							= "KGBD", // symbols: N,S,E,W
.not_set							= "AYARLANMADI",
.general_options					= "Genel Seçenekler",
.radio_options						= "Radyo Seç.",
.auto_night							= "Ot. Gece", // MaxLen: 16 (with .on or .off)
.dmr_rx_agc							= "DMR Rx AGC",
.speaker_click_suppress				= "Klik Susturma",
.gps								= "GPS",
.end_only							= "Sadece Son",
.dmr_crc							= "DMR crc",
.eco								= "Ekonomi",
.safe_power_on						= "Güv. Açma", // MaxLen: 16 (with ':' + .on or .off)
.auto_power_off						= "Ot.Kapanma", // MaxLen: 16 (with ':' + 30/60/90/120/180 or .no)
.apo_with_rf						= "APO / RF", // MaxLen: 16 (with ':' + .yes or .no or .n_a)
.brightness_night					= "Gece Par.", // MaxLen: 16 (with : + 0..100 + %)
.freq_set_VHF                			= "Frek VHF",
.gps_acquiring						= "Al“n“yor",
.altitude							= "Rak“m",
.calibration           				= "Radyo Ayar“",
.freq_set_UHF                			= "Frek UHF",
.cal_frequency         			    = "Kal. Frek.",
.cal_pwr              			    = "Kal. Gücü",
.pwr_set               				= "Güç Düzeltme",
.factory_reset         				= "Fab. Ayar“",
.rx_tune							= "Rx Ayarlama",
.transmitTalkerAliasTS2				= "TA Tx TS2", // Maxlen 16 (with : + .ta_text, 'APRS' , .both or .off)
.ta_text							= "Metin",
.daytime_theme_day					= "Gündüz Temas“", // MaxLen: 16
.daytime_theme_night				= "Gece Temas“", // MaxLen: 16
.theme_chooser						= "Tema Seçici", // Maxlen: 16
.theme_options						= "Tema Seç.",
.theme_fg_default					= "Metin Vars.", // MaxLen: 16 (+ colour rect)
.theme_bg							= "Arkaplan", // MaxLen: 16 (+ colour rect)
.theme_fg_decoration				= "Dekorasyon", // MaxLen: 16 (+ colour rect)
.theme_fg_text_input				= "Metin Giri–i", // MaxLen: 16 (+ colour rect)
.theme_fg_splashscreen				= "Aç“l“– ÖP", // MaxLen: 16 (+ colour rect)
.theme_bg_splashscreen				= "Aç“l“– AP", // MaxLen: 16 (+ colour rect)
.theme_fg_notification				= "Bil. Metni", // MaxLen: 16 (+ colour rect)
.theme_fg_warning_notification		= "Uyar“ Bil.", // MaxLen: 16 (+ colour rect)
.theme_fg_error_notification		= "Hata Bil.", // MaxLen: 16 (+ colour rect)
.theme_bg_notification              = "Bildirim AP", // MaxLen: 16 (+ colour rect)
.theme_fg_menu_name					= "Menü Ad“ ÖP", // MaxLen: 16 (+ colour rect)
.theme_bg_menu_name					= "Menü Ad“ AP", // MaxLen: 16 (+ colour rect)
.theme_fg_menu_item					= "Menü Ö˜eleri", // MaxLen: 16 (+ colour rect)
.theme_fg_menu_item_selected		= "Menü Vurgulama", // MaxLen: 16 (+ colour rect)
.theme_fg_options_value				= "Seçenek De˜eri", // MaxLen: 16 (+ colour rect)
.theme_fg_header_text				= "Ba–l“k Metni", // MaxLen: 16 (+ colour rect)
.theme_bg_header_text				= "Ba–l“k AP", // MaxLen: 16 (+ colour rect)
.theme_fg_rssi_bar					= "RSSI Çubu˜u", // MaxLen: 16 (+ colour rect)
.theme_fg_rssi_bar_s9p				= "RSSI Çub. S9+", // Maxlen: 16 (+colour rect)
.theme_fg_channel_name				= "Kanal Ad“", // MaxLen: 16 (+ colour rect)
.theme_fg_channel_contact			= "Ki–i", // MaxLen: 16 (+ colour rect)
.theme_fg_channel_contact_info		= "Ki–i Bilgisi", // MaxLen: 16 (+ colour rect)
.theme_fg_zone_name					= "Bölge Ad“", // MaxLen: 16 (+ colour rect)
.theme_fg_rx_freq					= "RX Frekans“", // MaxLen: 16 (+ colour rect)
.theme_fg_tx_freq					= "TX Frekans“", // MaxLen: 16 (+ colour rect)
.theme_fg_css_sql_values			= "CSS/SQL De˜.", // MaxLen: 16 (+ colour rect)
.theme_fg_tx_counter				= "TX Sayac“", // MaxLen: 16 (+ colour rect)
.theme_fg_polar_drawing				= "Kutup", // MaxLen: 16 (+ colour rect)
.theme_fg_satellite_colour			= "Uydu Noktas“", // MaxLen: 16 (+ colour rect)
.theme_fg_gps_number				= "GPS Numaras“", // MaxLen: 16 (+ colour rect)
.theme_fg_gps_colour				= "GPS Noktas“", // MaxLen: 16 (+ colour rect)
.theme_fg_bd_colour					= "BeiDou Nok.", // MaxLen: 16 (+ colour rect)
.theme_colour_picker_red			= "K“rm“z“", // MaxLen 16 (with ':' + 3 digits value)
.theme_colour_picker_green			= "Ye–il", // MaxLen 16 (with ':' + 3 digits value)
.theme_colour_picker_blue			= "Mavi", // MaxLen 16 (with ':' + 3 digits value)
.volume					= "Volume", // MaxLen: 8
.distance_sort				= "Dist sort", // MaxLen 16 (with ':' + .on or .off)
.show_distance				= "Show dist", // MaxLen 16 (with ':' + .on or .off)
.aprs_options				= "APRS options", // MaxLen 16
.aprs_smart				= "Smart", // MaxLen 16 (with ':' + .mode)
.aprs_channel				= "Channel", // MaxLen 16 (with ':' + .location)
.aprs_decay				= "Decay", // MaxLen 16 (with ':' + .on or .off)
.aprs_compress				= "Compress", // MaxLen 16 (with ':' + .on or .off)
.aprs_interval				= "Interval", // MaxLen 16 (with ':' + 0.2..60 + 'min')
.aprs_message_interval			= "Msg Interval", // MaxLen 16 (with ':' + 3..30)
.aprs_slow_rate				= "Slow Rate", // MaxLen 16 (with ':' + 1..100 + 'min')
.aprs_fast_rate				= "Fast Rate", // MaxLen 16 (with ':' + 10..180 + 's')
.aprs_low_speed				= "Low Speed", // MaxLen 16 (with ':' + 2..30 + 'km/h')
.aprs_high_speed			= "Hi Speed", // MaxLen 16 (with ':' + 2..90 + 'km/h')
.aprs_turn_angle			= "T. Angle", // MaxLen 16 (with ':' + 5..90 + <degree>)
.aprs_turn_slope			= "T. Slope", // MaxLen 16 (with ':' + 1..255 + '<degree>/v')
.aprs_turn_time				= "T. Time", // MaxLen 16 (with ':' + 5..180 + 's')
.auto_lock				= "Auto lock", // MaxLen 16 (with ':' + .off or 0.5..15 (.5 step) + 'min')
.trackball				= "Trackball", // MaxLen 16 (with ':' + .on or .off)
.dmr_force_dmo				= "Force DMO", // MaxLen 16 (with ':' + .n_a or .on or .off)
.encryption				= "Encryption", // MaxLen: 16
.enc_mode				= "Enc Mode", // MaxLen: 16
.enc_off				= "Off", // MaxLen: 16
.enc_arc4				= "ARC4", // MaxLen: 16
.enc_aes128				= "AES-128", // MaxLen: 16
.enc_scrambler				= "Scrambler", // MaxLen: 16
.scramble_id				= "Scram ID", // MaxLen: 16
};
/********************************************************************
 *
 * VERY IMPORTANT.
 * This file should not be saved with UTF-8 encoding
 * Use Notepad++ on Windows with ANSI encoding
 * or emacs on Linux with windows-1252-unix encoding
 *
 ********************************************************************/
#endif /* USER_INTERFACE_LANGUAGES_TURKISH_H_ */
