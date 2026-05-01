# Kombinle API Smoke Tests

Amaç:
Kritik API davranışlarını hızlıca doğrulamak.

## Senaryolar
1. business_meeting_formal_warning
2. casual_weekend_safe
3. interview_formal_safe

## Beklenenler
- Warning senaryosunda context-aware headline/subtext
- Safe senaryosunda generic safe message
- Alternative reasons catalog üzerinden çözülmeli
- Wardrobe feedback code/title/detail düzgün gelmeli

## Kullanım
Postman ile `/api/v1/decision` endpoint’ine ilgili request body gönderilir.