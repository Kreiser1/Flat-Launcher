namespace Database.Models
{
	/*
	CREATE TABLE IF NOT EXISTS categories (
		id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
		name VARCHAR(64) NOT NULL UNIQUE,
		CONSTRAINT CHK_category_name CHECK (name ~ '^[a-zA-Zа-яА-ЯёЁ0-9:]+(?: [a-zA-Zа-яА-ЯёЁ0-9:]+)*$' AND LENGTH(name) >= 3)
	); 
	*/

	public class Category
	{
		public required Guid Id { get; set; }
		public required string Name { get; set; }
	}
}
