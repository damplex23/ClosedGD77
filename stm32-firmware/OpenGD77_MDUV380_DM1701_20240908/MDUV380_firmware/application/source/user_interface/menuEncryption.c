/*
 * Copyright (C) 2026 ClosedGD77
 *
 * Encryption menu — global mode override for DMR/analog encryption.
 *
 * Items:
 *   1. Enc Mode — Off / ARC4 / AES-128 / Scrambler
 *   2. Scram ID — 1-8 (only active when Mode=Scrambler)
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 */
#include "user_interface/uiGlobals.h"
#include "user_interface/menuSystem.h"
#include "user_interface/uiLocalisation.h"
#include "user_interface/uiUtilities.h"
#include "encryption/encryption.h"

static void updateScreen(bool isFirstRun);
static void handleEvent(uiEvent_t *ev);

static menuStatus_t menuEncryptionExitCode = MENU_STATUS_SUCCESS;

enum
{
	ENC_MENU_MODE = 0,
	ENC_MENU_SCRAM_ID,
	NUM_ENC_MENU_ITEMS
};

// Number of valid modes (Off/ARC4/AES/Scrambler)
#if ENC_HAS_AES
#define NUM_ENC_MODES  4
#else
#define NUM_ENC_MODES  3  // Off/ARC4/Scrambler (no AES on MK22)
#endif

static uint8_t saved_global_mode;

menuStatus_t menuEncryption(uiEvent_t *ev, bool isFirstRun)
{
	if (isFirstRun)
	{
		menuDataGlobal.menuOptionsSetQuickkey = 0;
		menuDataGlobal.menuOptionsTimeout = 0;
		menuDataGlobal.newOptionSelected = true;
		menuDataGlobal.numItems = NUM_ENC_MENU_ITEMS;

		// Save current global mode for rollback on cancel
		saved_global_mode = encryption_get_global_mode();

		voicePromptsInit();
		voicePromptsAppendPrompt(PROMPT_SILENCE);
		voicePromptsAppendLanguageString(currentLanguage->encryption);
		voicePromptsAppendLanguageString(currentLanguage->menu);
		voicePromptsAppendPrompt(PROMPT_SILENCE);

		updateScreen(true);
		return (MENU_STATUS_LIST_TYPE | MENU_STATUS_SUCCESS);
	}
	else
	{
		menuEncryptionExitCode = MENU_STATUS_SUCCESS;

		if (ev->hasEvent || (menuDataGlobal.menuOptionsTimeout > 0))
		{
			handleEvent(ev);
		}
	}
	return menuEncryptionExitCode;
}

static void updateScreen(bool isFirstRun)
{
	int mNum = 0;
	char buf[SCREEN_LINE_BUFFER_SIZE];
	const char *leftSide = NULL;
	const char *rightSideConst = NULL;

	displayClearBuf();
	bool settingOption = uiQuickKeysShowChoices(buf, SCREEN_LINE_BUFFER_SIZE, currentLanguage->encryption);

	uint8_t mode = encryption_get_global_mode();
	uint8_t scram_id = encryption_get_scramble_id();

	for (int i = MENU_START_ITERATION_VALUE; i <= MENU_END_ITERATION_VALUE; i++)
	{
		if ((settingOption == false) || (i == 0))
		{
			mNum = menuGetMenuOffset(NUM_ENC_MENU_ITEMS, i);
			if (mNum == MENU_OFFSET_BEFORE_FIRST_ENTRY)
			{
				continue;
			}
			else if (mNum == MENU_OFFSET_AFTER_LAST_ENTRY)
			{
				break;
			}

			buf[0] = 0;
			buf[2] = 0;
			leftSide = NULL;
			rightSideConst = NULL;

			switch(mNum)
			{
				case ENC_MENU_MODE:
					leftSide = currentLanguage->enc_mode;
					if (mode == 0xFF) // not yet set — default Off
					{
						rightSideConst = currentLanguage->enc_off;
					}
					else if (mode == 0)
					{
						rightSideConst = currentLanguage->enc_off;
					}
					else if (mode == ENC_ALGO_ARC4) // 1
					{
						rightSideConst = currentLanguage->enc_arc4;
					}
					else if (mode == ENC_ALGO_AES128) // 2
					{
						rightSideConst = currentLanguage->enc_aes128;
					}
					else if (mode == ENC_ALGO_SCRAMBLER) // 4
					{
						rightSideConst = currentLanguage->enc_scrambler;
					}
					else
					{
						rightSideConst = currentLanguage->enc_off;
					}
					break;

				case ENC_MENU_SCRAM_ID:
					leftSide = currentLanguage->scramble_id;
					snprintf(buf, SCREEN_LINE_BUFFER_SIZE, "%d", (scram_id > 0 ? scram_id : 1));
					if (i == 0)
					{
						if ((!isFirstRun && menuDataGlobal.newOptionSelected) || isFirstRun)
						{
							voicePromptsInit();
							voicePromptsAppendLanguageString(leftSide);
							// ponytail: just read the number
							voicePromptsAppendPrompt((voicePrompt_t)(PROMPT_0 + (scram_id > 0 ? scram_id : 1)));
						}
					}
					menuDisplayEntry(i, mNum, buf, (strlen(leftSide) + 1),
						THEME_ITEM_FG_MENU_ITEM, THEME_ITEM_FG_OPTIONS_VALUE, THEME_ITEM_BG);
					continue;
			}

			snprintf(buf, SCREEN_LINE_BUFFER_SIZE, "%s:%s", leftSide, rightSideConst);

			if (i == 0)
			{
				bool wasPlaying = voicePromptsIsPlaying();

				if (!isFirstRun && (menuDataGlobal.menuOptionsSetQuickkey == 0))
				{
					voicePromptsInit();
				}

				if (!wasPlaying || (menuDataGlobal.newOptionSelected || (menuDataGlobal.menuOptionsTimeout > 0)))
				{
					voicePromptsAppendLanguageString(leftSide);
				}
				voicePromptsAppendLanguageString(rightSideConst);

				if (menuDataGlobal.menuOptionsTimeout != -1)
				{
					promptsPlayNotAfterTx();
				}
				else
				{
					menuDataGlobal.menuOptionsTimeout = 0;
				}
			}

			menuDisplayEntry(i, mNum, buf, (strlen(leftSide) + 1),
				THEME_ITEM_FG_MENU_ITEM, THEME_ITEM_FG_OPTIONS_VALUE, THEME_ITEM_BG);
		}
	}

	displayRender();
}

// Cycle the encryption mode: Off -> ARC4 -> AES -> Scrambler -> Off
static uint8_t cycle_mode_forward(uint8_t current)
{
	if (current >= 0xFE) return ENC_ALGO_ARC4; // unset -> ARC4
	switch (current)
	{
		case 0:                                          return ENC_ALGO_ARC4;      // Off -> ARC4
		case ENC_ALGO_ARC4:      /* 1 */
#if ENC_HAS_AES
			return ENC_ALGO_AES128;    // 2   ARC4 -> AES-128
		case ENC_ALGO_AES128:    /* 2 */                 return ENC_ALGO_SCRAMBLER;  // 4   AES -> Scrambler
#else
			return ENC_ALGO_SCRAMBLER; // 4   ARC4 -> Scrambler (skip AES on MK22)
#endif
		case ENC_ALGO_SCRAMBLER: /* 4 */                 return 0;                  // Scrambler -> Off
		default:                                        return 0;                  // Off
	}
}

static uint8_t cycle_mode_backward(uint8_t current)
{
	if (current >= 0xFE) return ENC_ALGO_SCRAMBLER; // unset -> Scrambler
	switch (current)
	{
		case 0:                                          return ENC_ALGO_SCRAMBLER; // Off -> Scrambler
		case ENC_ALGO_ARC4:      /* 1 */                 return 0;                  // ARC4 -> Off
		case ENC_ALGO_AES128:    /* 2 */                 return ENC_ALGO_ARC4;      // AES -> ARC4
		case ENC_ALGO_SCRAMBLER: /* 4 */
#if ENC_HAS_AES
			return ENC_ALGO_AES128;    // Scrambler -> AES
#else
			return ENC_ALGO_ARC4;      // Scrambler -> ARC4 (skip AES on MK22)
#endif
		default:                                        return 0;                  // Off
	}
}

static void handleEvent(uiEvent_t *ev)
{
	bool isDirty = false;

	if (ev->events & BUTTON_EVENT)
	{
		if (repeatVoicePromptOnSK1(ev))
		{
			return;
		}
	}

	if ((menuDataGlobal.menuOptionsTimeout > 0) && (!BUTTONCHECK_DOWN(ev, BUTTON_SK2)))
	{
		if (voicePromptsIsPlaying() == false)
		{
			menuDataGlobal.menuOptionsTimeout--;
			if (menuDataGlobal.menuOptionsTimeout == 0)
			{
				menuSystemPopPreviousMenu();
				return;
			}
		}
	}

	if (ev->events & FUNCTION_EVENT)
	{
		isDirty = true;
		if (ev->function == FUNC_REDRAW)
		{
			updateScreen(false);
			return;
		}
		else if ((QUICKKEY_TYPE(ev->function) == QUICKKEY_MENU) && (QUICKKEY_ENTRYID(ev->function) < NUM_ENC_MENU_ITEMS))
		{
			menuDataGlobal.currentItemIndex = QUICKKEY_ENTRYID(ev->function);
		}

		if ((QUICKKEY_FUNCTIONID(ev->function) != 0))
		{
			menuDataGlobal.menuOptionsTimeout = 1000;
		}
	}

	if ((ev->events & KEY_EVENT) && (menuDataGlobal.menuOptionsSetQuickkey == 0) && (menuDataGlobal.menuOptionsTimeout == 0))
	{
		if (KEYCHECK_PRESS(ev->keys, KEY_DOWN) && (menuDataGlobal.numItems != 0))
		{
			isDirty = true;
			menuSystemMenuIncrement(&menuDataGlobal.currentItemIndex, NUM_ENC_MENU_ITEMS);
			menuDataGlobal.newOptionSelected = true;
			menuEncryptionExitCode |= MENU_STATUS_LIST_TYPE;
		}
		else if (KEYCHECK_PRESS(ev->keys, KEY_UP))
		{
			isDirty = true;
			menuSystemMenuDecrement(&menuDataGlobal.currentItemIndex, NUM_ENC_MENU_ITEMS);
			menuDataGlobal.newOptionSelected = true;
			menuEncryptionExitCode |= MENU_STATUS_LIST_TYPE;
		}
		else if (KEYCHECK_SHORTUP(ev->keys, KEY_GREEN))
		{
			// Apply: global mode is already set in real-time. Pop to root.
			menuSystemPopAllAndDisplayRootMenu();
			return;
		}
		else if (KEYCHECK_SHORTUP(ev->keys, KEY_RED))
		{
			// Cancel: restore saved mode
			encryption_set_global_mode(saved_global_mode);
			menuSystemPopPreviousMenu();
			return;
		}
		else if (KEYCHECK_SHORTUP_NUMBER(ev->keys) && BUTTONCHECK_DOWN(ev, BUTTON_SK2))
		{
			menuDataGlobal.menuOptionsSetQuickkey = ev->keys.key;
			isDirty = true;
		}
	}

	if ((ev->events & (KEY_EVENT | FUNCTION_EVENT)) && (menuDataGlobal.menuOptionsSetQuickkey == 0))
	{
		if (KEYCHECK_PRESS(ev->keys, KEY_RIGHT)
#if defined(PLATFORM_RT84_DM1701) || defined(PLATFORM_MD2017)
				|| KEYCHECK_SHORTUP(ev->keys, KEY_ROTARY_INCREMENT)
#endif
				|| (QUICKKEY_FUNCTIONID(ev->function) == FUNC_RIGHT))
		{
			isDirty = true;
			menuDataGlobal.newOptionSelected = false;

			switch(menuDataGlobal.currentItemIndex)
			{
				case ENC_MENU_MODE:
				{
					uint8_t current = encryption_get_global_mode();
					uint8_t next = cycle_mode_forward(current);
					encryption_set_global_mode(next);
					break;
				}
				case ENC_MENU_SCRAM_ID:
				{
					uint8_t id = encryption_get_scramble_id();
					if (id == 0) id = 1;
					else if (id >= 8) id = 1;
					else id++;
					encryption_set_scramble_id(id);
					// Go to scrambler mode if cycling scram ID
					if (encryption_get_global_mode() != ENC_ALGO_SCRAMBLER)
					{
						encryption_set_global_mode(ENC_ALGO_SCRAMBLER);
					}
					break;
				}
			}
		}
		else if (KEYCHECK_PRESS(ev->keys, KEY_LEFT)
#if defined(PLATFORM_RT84_DM1701) || defined(PLATFORM_MD2017)
				|| KEYCHECK_SHORTUP(ev->keys, KEY_ROTARY_DECREMENT)
#endif
				|| (QUICKKEY_FUNCTIONID(ev->function) == FUNC_LEFT))
		{
			isDirty = true;
			menuDataGlobal.newOptionSelected = false;

			switch(menuDataGlobal.currentItemIndex)
			{
				case ENC_MENU_MODE:
				{
					uint8_t current = encryption_get_global_mode();
					uint8_t prev = cycle_mode_backward(current);
					encryption_set_global_mode(prev);
					break;
				}
				case ENC_MENU_SCRAM_ID:
				{
					uint8_t id = encryption_get_scramble_id();
					if (id <= 1) id = 8;
					else id--;
					encryption_set_scramble_id(id);
					if (encryption_get_global_mode() != ENC_ALGO_SCRAMBLER)
					{
						encryption_set_global_mode(ENC_ALGO_SCRAMBLER);
					}
					break;
				}
			}
		}
	}

	if (isDirty)
	{
		updateScreen(false);
	}
}
