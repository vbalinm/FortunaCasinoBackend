from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time

driver = webdriver.Chrome()
wait = WebDriverWait(driver, 10)

# 1) Nyisd meg a főoldalt
driver.get("http://localhost:5173")

# 2) Screenshot a bejelentkező oldalról
print("Bejelentkező oldal megnyitva.")
driver.save_screenshot("logintest_felulet.png")

# 3) Mezők beazonosítása
username = wait.until(EC.presence_of_element_located(
    (By.XPATH, "//input[@placeholder='felhasznalonev']")
))

password = wait.until(EC.presence_of_element_located(
    (By.XPATH, "//input[@autocomplete='current-password']")
))

# 4) Mezők kitöltése a login oldfalon
username.send_keys("admin")
password.send_keys("password123")

# 5) Screenshot kitöltés után
print("Bejelentkezési adatok megadva.")
time.sleep(0.5)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

# 6) Bejelentkezés gomb megnyomása
login_btn = wait.until(EC.element_to_be_clickable(
    (By.XPATH, "//button[contains(text(), 'Bejelentkezés')]")
))
login_btn.click()
time.sleep(0.5)  # Rövid szünet a screenshot után, hogy làtható legyen az eredmény

# 7) Screenshot a bejelentkező oldalról (bejelentkezés sikeres)
print("Bejelentkezés sikeres.")
driver.save_screenshot("admin_fooldal.png")
time.sleep(0.5)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön
admin_tab = wait.until(
    EC.element_to_be_clickable(
        (By.XPATH, "//a[contains(., 'Admin')]")
    )
)

admin_tab.click()
print("Admin fül megnyitva.")
time.sleep(0.5)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön
driver.save_screenshot("admin_felulet.png")

sorsolas_tab = wait.until(
    EC.element_to_be_clickable(
        (By.XPATH, "//button[contains(., 'Sorsolások')]")
    )
)

sorsolas_tab.click()
print("Sorsolások fül megnyitva.")
driver.save_screenshot("admin_sorsolas_felulet.png")
time.sleep(0.5)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

# --- Ötös Lottó → Sorsolás indítása ---
otos_btn = wait.until(
    EC.element_to_be_clickable(
        (By.XPATH, "//span[contains(., 'Ötös Lottó')]/ancestor::div[contains(@class,'justify-content-between')]//button[contains(., 'Sorsolás indítása')]")
    )
)

otos_btn.click()
print("Ötös Lottó sorsolás gomb megnyomva.")

# --- Confirm popup ---
alert = wait.until(EC.alert_is_present())
alert.accept()
print("Sorsolás megerősítve.")

time.sleep(4)
driver.save_screenshot("otos_sorsolas_siker.png")
print("Sorsolás sikeresen lefutott, screenshot elkészült.")
driver.quit()