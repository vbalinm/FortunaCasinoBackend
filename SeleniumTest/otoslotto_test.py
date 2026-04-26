from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time

driver = webdriver.Chrome()
wait = WebDriverWait(driver, 10)

# 1) Nyisd meg a főoldalt
driver.get("http://localhost:5173")

# 2) Bejelentkezés
username = wait.until(EC.presence_of_element_located(
    (By.XPATH, "//input[@placeholder='felhasznalonev']")
))

password = wait.until(EC.presence_of_element_located(
    (By.XPATH, "//input[@autocomplete='current-password']")
))

username.send_keys("admin")
password.send_keys("password123")

login_btn = wait.until(EC.element_to_be_clickable(
    (By.XPATH, "//button[contains(text(), 'Bejelentkezés')]")
))
login_btn.click()
time.sleep(0.5) # Rövid szünet a bejelentkezés után, hogy látható legyen az eredmény
print("Bejelentkezés sikeres.")

# 3) Screenshot az főoldalról
driver.save_screenshot("fooldal_felulet.png")

# 4) Ötöslottó oldalra navigálás
otos_card = wait.until(
    EC.element_to_be_clickable(
        (By.XPATH, "//h4[contains(text(), 'Ötös Lottó')]/ancestor::div[contains(@class,'game-card')]")
    )
)
otos_card.click()

# 5) Screenshot az Ötöslottó oldalról
print("Ötöslottó oldal megnyitva.")
driver.save_screenshot("otoslottoszelvény_felulet.png")

# 6) Ötöslottó szelvény kitöltése
# Joker rész kitöltése manuálisan és gyorstipp opcióval
first_joker_input = wait.until(
    EC.presence_of_element_located(
        (By.XPATH, "(//input[contains(@class,'joker-input')])[1]")
    )
)
first_joker_input.clear()
first_joker_input.send_keys("260522")

second_joker_quick = wait.until(
    EC.element_to_be_clickable(
        (By.XPATH, "(//div[contains(@class,'joker-row')])[2]//button")
    )
)
second_joker_quick.click()
print("Joker rész kitöltve.")

time.sleep(0.5) # Rövid szünet a kitöltés után, hogy látható legyen az eredmény

# Lottó rész kitöltése manuálisan és gyorstipp opcióval
# --- 1. mező: kezi számok kiválasztása ---
manual_numbers = [5, 22, 25, 64, 85]

for num in manual_numbers:
    btn = wait.until(
        EC.element_to_be_clickable(
            (By.XPATH, f"//button[normalize-space(text())='{num}']")
        )
    )
    btn.click()
    time.sleep(0.2)


# --- 2. mező: gyorstipp ---
second_field_quick = wait.until(
    EC.element_to_be_clickable(
        (By.XPATH, "(//button[contains(@class,'bg-blue-500')])[2]")
    )
)
second_field_quick.click()
print("2 mező Ötöslottó szelvény kitöltve.")
time.sleep(1)

# További 1 gépi játék kérése
# --- 1 gépi játék beállítása ---
machine_input = wait.until(
    EC.presence_of_element_located(
        (By.XPATH, "//input[@type='number' and @min='0' and @max='14']")
    )
)

machine_input.clear()
machine_input.send_keys("1")

# --- Generálás gomb megnyomása ---
generate_btn = wait.until(
    EC.element_to_be_clickable(
        (By.XPATH, "//button[contains(text(),'Generálás')]")
    )
)

generate_btn.click()
print("1 gépi játék generálva.")

time.sleep(1.5)  # hogy a gépi játék megjelenjen

# 7) Screenshot kitöltés után
driver.save_screenshot("otoslottoszelvény_felulet_kitoltes_utan.png")

# 8) Ötöslottó szelvény feladása (dupla kattintás)
play_button = wait.until(
    EC.element_to_be_clickable(
        (By.XPATH, "//button[contains(@class,'btn-green')]")
    )
)
play_button.click()
play_button.click()

time.sleep(1)
driver.save_screenshot("feladott_szelveny.png")
print("Ötöslottó szelvény feladás teszt sikeresen lefutott.")

driver.quit()