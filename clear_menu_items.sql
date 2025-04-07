USE allhourscafe_db;

-- Delete all menu items
DELETE FROM MenuItems;

-- Reset the auto-increment counter
ALTER TABLE MenuItems AUTO_INCREMENT = 1;
