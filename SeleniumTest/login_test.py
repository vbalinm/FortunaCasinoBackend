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
time.sleep(2)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

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
driver.save_screenshot("logintest_kitoltes_utan.png")
time.sleep(2)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

# 6) Bejelentkezés gomb megnyomása
login_btn = wait.until(EC.element_to_be_clickable(
    (By.XPATH, "//button[contains(text(), 'Bejelentkezés')]")
))
login_btn.click()
time.sleep(2)  # Rövid szünet a screenshot után, hogy làtható legyen az eredmény

# 7) Screenshot a bejelentkező oldalról (bejelentkezés sikeres)
print("Bejelentkezés sikeres.")
driver.save_screenshot("logintest_sikeres.png")
time.sleep(2)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

driver.quit()