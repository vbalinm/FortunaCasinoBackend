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
driver.save_screenshot("login_felulet.png")
time.sleep(3)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

# 3) Kattintás a "Regisztrálj most" linkre
reg_btn = wait.until(EC.element_to_be_clickable(
    (By.XPATH, "//button[contains(text(), 'Regisztrálj most')]")
))
reg_btn.click()


# 4) Várakozás, hogy betöltsön a regisztrációs oldal
wait.until(EC.presence_of_element_located((By.NAME, "username")))

# 5) Screenshot a regisztrációs oldalról (mezők kitöltése előtt)
print("Regisztrációs oldal megnyitva.")

driver.save_screenshot("regisztracio_kitoltes_elott.png")
time.sleep(3)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

# 6) Mezők kitöltése a regisztrációs oldalon
username = driver.find_element(By.NAME, "username")
email = driver.find_element(By.NAME, "email")
password = driver.find_element(By.NAME, "password")
confirm = driver.find_element(By.NAME, "confirmPassword")

username.send_keys("KormieTest123")
email.send_keys("kormos.levente@kkszki.hu")
password.send_keys("Password123")
confirm.send_keys("Password123")

# 7) Screenshot kitöltés után
print("Regisztrációs adatok megadva.")
driver.save_screenshot("regisztracio_kitoltes_után.png")
time.sleep(3)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

# 8) Regisztráció gomb megnyomása
submit_btn = driver.find_element(By.XPATH, "//button[contains(text(), 'Regisztráció')]")
submit_btn.click()
time.sleep(4)  # Rövid szünet a screenshot után, hogy látható legyen az eredmény

# 9) Screenshot a regisztrációs oldalról (regisztráció sikeres)
print("Regisztráció sikeres.")
driver.save_screenshot("regisztracio_sikeres.png")
time.sleep(2)  # Rövid szünet a screenshot előtt, hogy minden elem betöltődjön

driver.quit()
